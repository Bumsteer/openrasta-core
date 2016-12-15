#region License
/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */
#endregion

using System;
using System.Linq;
using NUnit.Framework;
using OpenRasta;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Pipeline.Diagnostics;
using OpenRasta.Testing;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace PipelineRunner_Specification
{
    public class when_creating_the_pipeline : pipelinerunner_context
    {
        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        [TestCase(typeof(TopologicalSortCallGraphGenerator))]
        public void a_registered_contributor_gets_initialized_and_is_part_of_the_contributor_collection(Type callGraphGeneratorType)
        {
            var pipeline = CreatePipeline(callGraphGeneratorType, new[]
            {
                typeof(DummyContributor)
            }, false);
            pipeline.Contributors.OfType<DummyContributor>().FirstOrDefault()
                .ShouldNotBeNull();
        }

        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        [TestCase(typeof(TopologicalSortCallGraphGenerator))]
        public void valid_pipeline_is_required(Type callGraphGeneratorType)
        {
            Executing(()=>CreatePipeline(callGraphGeneratorType, new[]
            {
                typeof(DummyContributor)
            })).ShouldThrow<DependentContributorMissingException>()
            .ContributorTypes
                .ShouldHaveCountOf(typeof(KnownStages).GetNestedTypes().Length - 1);
        }
        public class DummyContributor : AfterContributor<KnownStages.IBegin>
        {
        }
    }

    public class when_accessing_the_contributors : pipelinerunner_context
    {
        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        [TestCase(typeof(TopologicalSortCallGraphGenerator))]
        public void the_contributor_list_always_contains_the_bootstrap_contributor(Type callGraphGeneratorType)
        {
            var pipeline = CreatePipeline(callGraphGeneratorType, new Type[] { }, false);

            pipeline.Contributors.OfType<KnownStages.IBegin>().FirstOrDefault()
                .ShouldNotBeNull();
        }

        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        [TestCase(typeof(TopologicalSortCallGraphGenerator))]
        public void the_contributor_list_is_read_only(Type callGraphGeneratorType)
        {
            CreatePipeline(callGraphGeneratorType, new Type[] { }, false)
                .Contributors.IsReadOnly
                .ShouldBeTrue();
        }
    }

    public class when_building_the_call_graph : pipelinerunner_context
    {
        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        [TestCase(typeof(TopologicalSortCallGraphGenerator))]
        public void a_second_contrib_registering_after_the_first_contrib_that_registers_after_the_boot_initializes_the_call_list_in_the_correct_order(Type callGraphGeneratorType)
        {
            var pipeline = CreatePipeline(callGraphGeneratorType, new []
            {
                typeof (SecondIsAfterFirstContributor),
                typeof (FirstIsAfterBootstrapContributor)
            }, false);

            pipeline.CallGraph.ShouldHaveSameElementsAs(new[]
            {
                typeof (BootstrapperContributor),
                typeof (FirstIsAfterBootstrapContributor),
                typeof (SecondIsAfterFirstContributor)
            }, (a, b) => a.Target.GetType() == b);
        }

        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        public void registering_all_the_contributors_results_in_a_correct_call_graph(Type callGraphGeneratorType)
        {
            var pipeline = CreatePipeline(callGraphGeneratorType, new[]
            {
                typeof(FirstIsAfterBootstrapContributor),
                typeof(SecondIsAfterFirstContributor),
                typeof(ThirdIsBeforeFirstContributor),
                typeof(FourthIsAfterThirdContributor)
            }, false);

            pipeline.CallGraph.ShouldHaveSameElementsAs(new[]
            {
                typeof(BootstrapperContributor),
                typeof(ThirdIsBeforeFirstContributor),
                typeof(FourthIsAfterThirdContributor),
                typeof(FirstIsAfterBootstrapContributor),
                typeof(SecondIsAfterFirstContributor)
            }, (a, b) => a.Target.GetType() == b);
        }

        [Test]
        public void registering_all_the_contributors_results_in_a_correct_call_graph_topological()
        {
            var pipeline = CreatePipeline(typeof(TopologicalSortCallGraphGenerator), new[]
            {
                typeof(FirstIsAfterBootstrapContributor),
                typeof(SecondIsAfterFirstContributor),
                typeof(ThirdIsBeforeFirstContributor),
                typeof(FourthIsAfterThirdContributor)
            }, false);

            pipeline.CallGraph.ShouldHaveSameElementsAs(new[]
            {
                typeof(BootstrapperContributor),
                typeof(ThirdIsBeforeFirstContributor),
                typeof(FirstIsAfterBootstrapContributor),
                typeof(SecondIsAfterFirstContributor),
                typeof(FourthIsAfterThirdContributor)
            }, (a, b) => a.Target.GetType() == b);
        }

        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        [TestCase(typeof(TopologicalSortCallGraphGenerator))]
        public void the_call_graph_cannot_be_recursive(Type callGraphGeneratorType)
        {
            Executing(() => CreatePipeline(callGraphGeneratorType, new[]
            {
                typeof(RecursiveA), typeof(RecursiveB)
            }, false)).ShouldThrow<RecursionException>();
        }

        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        [TestCase(typeof(TopologicalSortCallGraphGenerator))]
        public void registering_contributors_with_multiple_recursive_notifications_should_be_identified_as_invalid(Type callGraphGeneratorType)
        {
            Executing(() => CreatePipeline(callGraphGeneratorType, new[]
            {
                typeof(ContributorA),
                typeof(ContributorB),
                typeof(ContributorC)
            }, false)).ShouldThrow<RecursionException>();
        }

        public static PipelineContinuation DoNothing(ICommunicationContext c)
        {
            return PipelineContinuation.Continue;
        }

        public class ContributorA : IPipelineContributor
        {
            public void Initialize(IPipeline pipelineRunner)
            {
                pipelineRunner.Notify(DoNothing).After<KnownStages.IBegin>();
            }
        }

        public class ContributorB : IPipelineContributor
        {
            public void Initialize(IPipeline pipelineRunner)
            {
                pipelineRunner.Notify(DoNothing).After<ContributorA>();
                pipelineRunner.Notify(DoNothing).After<ContributorC>();
            }
        }

        public class ContributorC : IPipelineContributor
        {
            public void Initialize(IPipeline pipelineRunner)
            {
                pipelineRunner.Notify(DoNothing).After<ContributorB>();
            }
        }

        public class AfterAnyContributor : AfterContributor<IPipelineContributor>
        {
        }

        public class FirstIsAfterBootstrapContributor : AfterContributor<KnownStages.IBegin>
        {
        }

        public class FourthIsAfterThirdContributor : AfterContributor<ThirdIsBeforeFirstContributor>
        {
        }

        public class RecursiveA : IPipelineContributor
        {

            public void Initialize(IPipeline pipelineRunner)
            {
                pipelineRunner.Notify(DoNothing).After<KnownStages.IBegin>().And.After<RecursiveB>();
            }
        }

        public class RecursiveB : AfterContributor<RecursiveA>
        {
        }

        public class SecondIsAfterFirstContributor : AfterContributor<FirstIsAfterBootstrapContributor>
        {
        }

        public class SecondIsBeforeFirstContributor : BeforeContributor<FirstIsAfterBootstrapContributor>
        {
        }
        public class ThirdIsBeforeFirstContributor : BeforeContributor<FirstIsAfterBootstrapContributor>
        {
        }
    }

    public class when_contributor_throws : pipelinerunner_context
    {
        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        [TestCase(typeof(TopologicalSortCallGraphGenerator))]
        public void error_is_collected_and_500_returned(Type callGraphGeneratorType)
        {
            var pipeline = CreatePipeline(callGraphGeneratorType, new[]
            {
                typeof(ContributorThatThrows),
                typeof(FakeOperationResultInvoker)
            }, false);

            var context = new InMemoryCommunicationContext();
            pipeline.Run(context);
            context.Response.StatusCode.ShouldBe(500);
            context.ServerErrors.ShouldHaveCountOf(1);
        }

        public class FakeOperationResultInvoker : KnownStages.IOperationResultInvocation
        {
            public void Initialize(IPipeline pipelineRunner)
            {
                pipelineRunner.Notify(DoNowt).After<ContributorThatThrows>();
            }

            PipelineContinuation DoNowt(ICommunicationContext arg)
            {
                return PipelineContinuation.Continue;
            }
        }

        class ContributorThatThrows : IPipelineContributor
        {
            public void Initialize(IPipeline pipelineRunner)
            {
                pipelineRunner.Notify(ctx =>
                {
                    throw new NotImplementedException();
                }).After<KnownStages.IBegin>();
            }
        }
    }
    public class when_executing_the_pipeline : pipelinerunner_context
    {
        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        [TestCase(typeof(TopologicalSortCallGraphGenerator))]
        public void contributors_get_executed(Type callGraphGeneratorType)
        {
            var pipeline = CreatePipeline(callGraphGeneratorType, new[]
            {
                typeof(WasCalledContributor)
            }, false);

            pipeline.Run(new InMemoryCommunicationContext());
            WasCalledContributor.WasCalled.ShouldBeTrue();
        }

        [TestCase(null)]
        [TestCase(typeof(DefaultCallGraphGenerator))]
        [TestCase(typeof(TopologicalSortCallGraphGenerator))]
        public void the_pipeline_must_have_been_initialized(Type callGraphGeneratorType)
        {
            var pipeline = new PipelineRunner(new InternalDependencyResolver());
            Executing(() => pipeline.Run(new InMemoryCommunicationContext()))
                .ShouldThrow<InvalidOperationException>();
        }

        public class WasCalledContributor : IPipelineContributor
        {
            public static bool WasCalled;

            public PipelineContinuation Do(ICommunicationContext context)
            {
                WasCalled = true;
                return PipelineContinuation.Continue;
            }

            public void Initialize(IPipeline pipelineRunner)
            {
                pipelineRunner.Notify(Do).After<KnownStages.IBegin>();
            }
        }
    }

    public class pipelinerunner_context : context
    {
        protected IPipeline CreatePipeline(Type callGraphGeneratorType, Type[] contributorTypes, bool validate=true)
        {
            var resolver = new InternalDependencyResolver();
            resolver.AddDependency<IPipelineContributor, BootstrapperContributor>();

            if (callGraphGeneratorType != null)
            {
                resolver.AddDependency(typeof(IGenerateCallGraphs), callGraphGeneratorType, DependencyLifetime.Singleton);
            }

            foreach (var type in contributorTypes)
                resolver.AddDependency(typeof(IPipelineContributor), type, DependencyLifetime.Singleton);
            var runner = new PipelineRunner(resolver) { PipelineLog = new TraceSourceLogger<PipelineLogSource>() };
            runner.Initialize(validate);
            return runner;
        }
    }

    public class AfterContributor<T> : IPipelineContributor where T : IPipelineContributor
    {
        public PipelineContinuation DoNothing(ICommunicationContext c)
        {
            return PipelineContinuation.Continue;
        }

        public virtual void Initialize(IPipeline pipelineRunner)
        {
            pipelineRunner.Notify(DoNothing).After<T>();
        }
    }

    public class BeforeContributor<T> : IPipelineContributor where T : IPipelineContributor
    {
        public PipelineContinuation DoNothing(ICommunicationContext c)
        {
            return PipelineContinuation.Continue;
        }

        public virtual void Initialize(IPipeline pipelineRunner)
        {
            pipelineRunner.Notify(DoNothing).Before<T>();
        }
    }
}

#region Full license
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion
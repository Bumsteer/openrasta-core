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
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Testing;
using OpenRasta.Testing.Contexts;
using OpenRasta.Tests;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.Web;
using OpenRasta.Pipeline;
using Shouldly;

namespace ResponseEntityCodecResolver_Specification
{
    public class when_a_codec_is_already_defined : openrasta_context
    {
        [Test]
        public void the_codec_is_not_changed_and_the_pipeline_continues()
        {
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(new Customer(), typeof (Codec), "application/xml");
            given_registration_codec<CustomerCodec, Customer>("text/plain");
            given_request_header_accept("text/plain");

          when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.Continue);
          //return valueToAnalyse;

          Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(Codec));
        }
        [Test]
        public void there_is_no_vary_header()
        {
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(new Customer(), typeof(Codec), "application/xml");
            given_registration_codec<CustomerCodec, Customer>("text/plain");
            given_request_header_accept("text/plain");

          when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.Continue);
          //return valueToAnalyse;

          Context.Response.Entity.Headers["Vary"].ShouldBeNull();
        }
    }

    public class when_no_codec_has_been_predefined_defined : openrasta_context
    {
        [Test]
        public void a_semi_wildcard_gets_priority_over_a_full_wildcard()
        {
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(new Customer());
            given_registration_codec<CustomerCodec, Customer>("text/plain");
            given_registration_codec<AnotherCustomerCodec, Customer>("application/xml");
            given_request_header_accept("text/*,*/*");

            when_running_pipeline();

          Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
          ShouldBeTestExtensions.ShouldBe(Context.Response.Entity.ContentType.MediaType, "text/plain");
          //return valueToAnalyse;
        }

        [Test]
        public void bad_request_is_returned_when_the_accept_header_has_only_one_entry_that_is_invalid()
        {
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(new Customer());
            given_registration_codec<CustomerCodec, Customer>("text/plain");
            given_request_header_accept("q=");

          when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.RenderNow);
          //return valueToAnalyse;

          Context.OperationResult.ShouldBeAssignableTo<OperationResult.BadRequest>();
          ShouldBeTestExtensions.ShouldBe(Context.Response.Headers["Warning"], "199 Malformed accept header");
          //return valueToAnalyse;
        }

        [Test]
        public void ignore_an_invalid_entry_if_accept_header_has_mixture_of_valid_and_invalid_values()
        {
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(new Customer());
            given_registration_codec<CustomerCodec, Customer>("text/plain");
            given_request_header_accept("text/plain,q=");

            when_running_pipeline();

          Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
          ShouldBeTestExtensions.ShouldBe(Context.Response.Entity.ContentType.MediaType, "text/plain");
          //return valueToAnalyse;
        }

        [Test]
        public void an_error_is_returned_when_no_suitable_codec_is_found()
        {
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(new Customer());
            given_registration_codec<CustomerCodec, Customer>("text/plain");
            given_request_header_accept("application/xml");

          when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.RenderNow);
          //return valueToAnalyse;


          Context.OperationResult.ShouldBeAssignableTo<OperationResult.ResponseMediaTypeUnsupported>();
        }

        [Test]
        public void nothing_happens_for_a_null_entity()
        {
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(null);
            given_registration_codec<CustomerCodec, Customer>("text/plain");
            given_request_header_accept("text/plain");

            when_running_pipeline();

          Context.PipelineData.ResponseCodec.ShouldBeNull();
          Context.Response.Entity.ContentType.ShouldBeNull();
        }

        [Test]
        public void the_codec_is_selected_for_an_exact_match()
        {
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(new Customer());
            given_registration_codec<CustomerCodec, Customer>("text/plain");
            given_registration_codec<Codec, Customer>("application/xml");
            given_request_header_accept("text/plain");

            when_running_pipeline();

          Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
          ShouldBeTestExtensions.ShouldBe(Context.Response.Entity.ContentType.MediaType, "text/plain");
          //return valueToAnalyse;
        }

        [Test]
        public void the_server_quality_is_respected_when_the_accept_header_has_a_full_wildcard()
        {
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(new Customer());
            given_registration_codec<CustomerCodec, Customer>("text/plain;q=0.9");
            given_registration_codec<AnotherCustomerCodec, Customer>("application/xml");
            given_request_header_accept("*/*");
            when_running_pipeline();


          Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(AnotherCustomerCodec));
          ShouldBeTestExtensions.ShouldBe(Context.Response.Entity.ContentType.MediaType, "application/xml");
          //return valueToAnalyse;
        }
        [Test]
        public void the_client_quality_parameter_is_respected()
        {
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(new Customer());
            given_registration_codec<CustomerCodec, Customer>("text/plain;q=0.9");
            given_registration_codec<AnotherCustomerCodec, Customer>("application/xml");
            given_request_header_accept("text/plain,application/xml;q=0.1");

            when_running_pipeline();

          Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
          ShouldBeTestExtensions.ShouldBe(Context.Response.Entity.ContentType.MediaType, "text/plain");
          //return valueToAnalyse;
        }

        void when_running_pipeline()
        {
          when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.Continue);
          //return valueToAnalyse;
        }

        [Test,Category("Regression")]
        public void the_server_quality_is_used_to_select_the_closest_media_type()
        {
            // Regression from ticket #54
            given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
            given_response_entity(new Customer());
            given_registration_codec<CustomerCodec, Customer>("application/xhtml+xml;q=0.9,text/html,application/vnd.openrasta.htmlfragment+xml;q=0.5");
            given_request_header_accept("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

          when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.Continue);
          //return valueToAnalyse;

          Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
          ShouldBeTestExtensions.ShouldBe(Context.Response.Entity.ContentType.MediaType, "text/html");
          //return valueToAnalyse;
        }
    }
}

#region Full license
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#endregion
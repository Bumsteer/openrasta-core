﻿using System.Linq;
using NUnit.Framework;
using OpenRasta.OperationModel;
using OpenRasta.Testing;
using OpenRasta.Testing.Contexts;
using OpenRasta.TypeSystem;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased
{
    public class when_using_optional_members : operation_context<MockOperationHandler>
    {
        [Test]
        public void the_operation_is_ready_for_invocation()
        {
            given_operation("Get", typeof(int));

            Operation.Inputs.AllReady().LegacyShouldBeTrue();
        }
        [Test]
        public void all_parameters_are_satisfied()
        {
            given_operation("Get", typeof(int));

            Operation.Inputs.CountReady().LegacyShouldBe(1);
        }
        [Test]
        public void a_default_parameter_value_is_supported()
        {
            given_operation("Search",typeof(string));

            Operation.Inputs.Optional().First().IsOptional.LegacyShouldBeTrue();
            Operation.Inputs.Optional().First().Member.LegacyShouldBeOfType<IParameter>().DefaultValue
                .LegacyShouldBe("*");
        }
    }

  public class when_using_native_optional_members : operation_context<MockOperationHandler>
  {

    [Test]
    public void the_operation_is_ready_for_invocation()
    {
      given_operation("SearchNative", typeof(string));

      Operation.Inputs.AllReady().LegacyShouldBeTrue();
    }

    [Test]
    public void all_parameters_are_satisfied()
    {
      given_operation("SearchNative", typeof(string));

      Operation.Inputs.CountReady().LegacyShouldBe(1);
    }

    [Test]
    public void a_default_parameter_value_is_supported()
    {
      given_operation("SearchNative", typeof(string));

      Operation.Inputs.Optional().First().IsOptional.LegacyShouldBeTrue();
      Operation.Inputs.Optional().First().Member.LegacyShouldBeOfType<IParameter>().DefaultValue
        .LegacyShouldBe("*");
    }
  }
}

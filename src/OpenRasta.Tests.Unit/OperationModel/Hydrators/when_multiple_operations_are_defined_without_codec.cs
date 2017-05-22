﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Binding;
using OpenRasta.Codecs;
using OpenRasta.Diagnostics;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Hydrators;
using OpenRasta.OperationModel.Hydrators.Diagnostics;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.Tests.Unit.OperationModel.Filters;
using OpenRasta.TypeSystem;
using OpenRasta.Web;
using OpenRasta.Web.Codecs;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.Hydrators
{
  public class when_multiple_operations_are_defined_without_codec : request_entity_reader_context
  {
    [Test]
    public void multiple_equivalent_operations_throws()
    {
      given_filter();
      given_operations();

      given_operation_value("PostName", "frodo", new Frodo());
      given_operation_value("PostStream", "anObject", Stream.Null);

      when_entity_is_read();

      Error.ShouldBeAssignableTo<AmbiguousRequestException>();
    }

    [Test]
    public void the_one_with_the_highest_number_of_satisfied_parameters_and_ready_for_invocation_is_selected()
    {
      given_filter();
      given_operations();

      given_operation_value("PostName", "frodo", new Frodo());

      given_operation_value("PostAddress", "frodo", new Frodo());
      given_operation_value("PostAddress", "address", new Address());

      when_entity_is_read();

      ShouldBeTestExtensions.ShouldBe(ResultOperation.Name, "PostAddress");
      //return valueToAnalyse;
    }
  }
}

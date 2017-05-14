﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections;
using OpenRasta.Collections.Specialized;

namespace OpenRasta.Pipeline.CallGraph
{
  public sealed class TopologicalSortCallGraphGenerator : IGenerateCallGraphs
  {
      public IEnumerable<ContributorCall> GenerateCallGraph(IEnumerable<IPipelineContributor> contributors)
    {
      contributors = contributors.ToList();

      var bootstrapper =
        contributors
          .OfType<KnownStages.IBegin>()
          .SingleOrDefault() ?? throw new InvalidOperationException("No IBegin contributor found.");

      var nodes = new List<TopologicalNode<ContributorNotification>>();

      foreach (var contributor in contributors.Where(x => x != bootstrapper))
      {
        var contributorBuilder = new ContributorInitializer(contributors);
        var builder = new CompatibilityContributorInitializer(contributorBuilder);

        contributor.Initialize(builder);

        nodes.AddRange(
          contributorBuilder.ContributorRegistrations
              .DefaultIfEmpty(new Notification(
                  Middleware.IdentitySingleTap,
                  contributorBuilder.Contributors))
              .Select(reg => new TopologicalNode<ContributorNotification>(
                  new ContributorNotification(contributor, reg))));
      }

      foreach (var notificationNode in nodes)
      {
        foreach (var afterType in notificationNode.Item.Notification.AfterTypes)
        {
          var parents = GetCompatibleNodes(nodes, notificationNode, afterType);
          notificationNode.Dependencies.AddRange(parents);
        }

        foreach (var beforeType in notificationNode.Item.Notification.BeforeTypes)
        {
          var children = GetCompatibleNodes(nodes, notificationNode, beforeType);
          foreach (var child in children)
          {
            child.Dependencies.Add(notificationNode);
          }
        }
      }

      var rootItem = new ContributorNotification(bootstrapper,
          new Notification(Middleware.IdentitySingleTap, contributors));

      return new TopologicalTree<ContributorNotification>(rootItem, nodes).Nodes
        .Select(
          n => new ContributorCall(n.Item.Contributor, n.Item.Notification.Target, n.Item.Notification.Description));
    }

    static IEnumerable<TopologicalNode<ContributorNotification>> GetCompatibleNodes(
      IEnumerable<TopologicalNode<ContributorNotification>> nodes,
      TopologicalNode<ContributorNotification> notificationNode, Type type)
    {
      return from compatibleNode in nodes
        where !compatibleNode.Equals(notificationNode) && type.IsInstanceOfType(compatibleNode.Item.Contributor)
        select compatibleNode;
    }
  }
}

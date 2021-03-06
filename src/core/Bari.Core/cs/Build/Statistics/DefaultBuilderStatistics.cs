﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Bari.Core.Build.Statistics
{
	public class DefaultBuilderStatistics: IBuilderStatistics
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DefaultBuilderStatistics));
		private readonly IDictionary<Type, BuilderStats> builderStats = new Dictionary<Type, BuilderStats>();

		public void Add(Type builderType, string description, TimeSpan elapsed)
		{
			BuilderStats stats;
			if (!builderStats.TryGetValue(builderType, out stats))
			{
				stats = new BuilderStats(builderType);
				builderStats.Add(builderType, stats);
			}

			stats.Add(description, elapsed);
		}

		public void Dump()
		{
			log.Debug("Builder performance statistics");
			log.Debug("----");

			var byTotal = builderStats.OrderByDescending(kv => kv.Value.Total);
			foreach (var item in byTotal)
			{
				log.DebugFormat("# {0} ({1}x) => total: {2:F3}s, average: {3:F3}s", FormatType(item.Key), item.Value.Count, item.Value.Total.TotalSeconds, item.Value.Average.TotalSeconds);

				var records = item.Value.All.OrderByDescending(r => r.Length);
				foreach (var record in records)
				{
					log.DebugFormat("    - {0}: {1:F3}s", record.Id, record.Length.TotalSeconds);
				}
			}

			log.Debug("----");
		}

	    private string FormatType(Type type)
	    {
	        if (type.IsGenericType)
	        {
	            return String.Format("{0}<{1}>", type.Name, String.Join(", ", type.GetGenericArguments().Select(FormatType)));	        
	        }
	        else
	        {
	            return type.Name;
	        }
	    }
	}
}


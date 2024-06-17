﻿#nullable disable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BizHawk.Emulation.Common
{
	public interface IGameInfo
	{
		string Name { get; }
		string System { get; }

		/// <value>either CRC32, MD5, or SHA1, hex-encoded, unprefixed</value>
		string Hash { get; }

		string Region { get; }
		RomStatus Status { get; }
		bool NotInDatabase { get; }
		string FirmwareHash { get; }
		string ForcedCore { get; }
	}

	
	[DataContract]
	public class GameInfo : IGameInfo
	{
		[DataMember] public string Name { get; set; }
		[DataMember] public string System { get; set; }
		[DataMember] public string Hash { get; set; }
		[DataMember] public string Region { get; set; }
		[DataMember] public RomStatus Status { get; set; } = RomStatus.NotInDatabase;
		[DataMember] public bool NotInDatabase { get; set; } = true;
		[DataMember] public string FirmwareHash { get; set; }
		[DataMember] public string ForcedCore { get; set; }
		
		private Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();

		private static readonly JsonSerializer Serializer = new ()
		{
			MissingMemberHandling = MissingMemberHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto
			, ConstructorHandling = ConstructorHandling.Default,

			ObjectCreationHandling = ObjectCreationHandling.Replace
			, ContractResolver = new DefaultContractResolver
			{
				DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
			}
		};
		
		public GameInfo()
		{
		}

		public GameInfo Clone()
		{
			var ret = (GameInfo)MemberwiseClone();
			ret.Options = new Dictionary<string, string>(Options);
			return ret;
		}

		public static readonly GameInfo NullInstance = new()
		{
			Name = "Null",
			System = VSystemID.Raw.NULL,
			Hash = "",
			Region = "",
			Status = RomStatus.GoodDump,
			ForcedCore = "",
			NotInDatabase = false
		};

		internal GameInfo(CompactGameInfo cgi)
		{
			Name = cgi.Name;
			System = cgi.System;
			Hash = cgi.Hash;
			Region = cgi.Region;
			Status = cgi.Status;
			ForcedCore = cgi.ForcedCore;
			NotInDatabase = false;
			ParseOptionsDictionary(cgi.MetaData);
		}

		public void AddOption(string option, string param)
		{
			Options[option] = param;
		}

		public void RemoveOption(string option)
		{
			Options.Remove(option);
		}

		public bool this[string option] => Options.ContainsKey(option);

		public bool OptionPresent(string option)
		{
			return Options.ContainsKey(option);
		}

		public string OptionValue(string option)
		{
			return Options.TryGetValue(option, out var s) ? s : null;
		}

		public int GetIntValue(string option)
		{
			return int.Parse(Options[option]);
		}

		public string GetStringValue(string option)
		{
			return Options[option];
		}

		public int GetHexValue(string option)
		{
			return int.Parse(Options[option], NumberStyles.HexNumber);
		}

		/// <summary>
		/// /// Gets a boolean value from the database
		/// </summary>
		/// <param name="parameter">The option to look up</param>
		/// <param name="defaultVal">The value to return if the option is invalid or doesn't exist</param>
		/// <returns> The boolean value from the database if present, otherwise the given default value</returns>
		public bool GetBool(string parameter, bool defaultVal)
		{
			if (OptionValue(parameter) == "true")
			{
				return true;
			}

			if (OptionValue(parameter) == "false")
			{
				return false;
			}
			
			return defaultVal;
		}

		/// <summary>
		/// /// Gets an integer value from the database
		/// </summary>
		/// <param name="parameter">The option to look up</param>
		/// <param name="defaultVal">The value to return if the option is invalid or doesn't exist</param>
		/// <returns> The integer value from the database if present, otherwise the given default value</returns>
		public int GetInt(string parameter, int defaultVal)
		{
			if (Options.ContainsKey(parameter))
			{
				try
				{
					return int.Parse(OptionValue(parameter));
				}
				catch
				{
					return defaultVal;
				}
			}

			return defaultVal;
		}

		public IReadOnlyDictionary<string, string> GetOptions()
		{
			return new ReadOnlyDictionary<string, string>(Options);
		}

		private void ParseOptionsDictionary(string metaData)
		{
			if (string.IsNullOrEmpty(metaData))
			{
				return;
			}

			var options = metaData.Split(';').Where(opt => string.IsNullOrEmpty(opt) == false).ToArray();

			foreach (var opt in options)
			{
				var parts = opt.Split('=');
				var key = parts[0];
				var value = parts.Length > 1 ? parts[1] : "";
				Options[key] = value;
			}
		}

		public void Serialize(TextWriter tw)
		{
			var jw = new JsonTextWriter(tw) { Formatting = Formatting.Indented };
			Serializer.Serialize(jw, this);
		}
		
		public static GameInfo Deserialize(Stream stream)
		{
			var sr = new StreamReader(stream);
			var jr = new JsonTextReader(sr);
			return Serializer.Deserialize<GameInfo>(jr);
		}
	}

	public static class GameInfoExtensions
	{
		public static bool IsNullInstance(this IGameInfo game)
		{
			return game == null || game.System == VSystemID.Raw.NULL;
		}

		public static bool IsRomStatusBad(this IGameInfo game)
		{
			return game.Status == RomStatus.BadDump || game.Status == RomStatus.Overdump;
		}
	}
}

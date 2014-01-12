using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConfOxide {
	///<summary>Contains extension methods for reading and writing <see cref="SettingsBase{T}"/> instances from files on disk.</summary>
	///<remarks>This class contains convenience wrappers around JSON.Net and file IO.</remarks>
	public static class SettingsFileExtensions {
		///<summary>Updates a <see cref="SettingsBase{T}"/> instance from a JSON file, if it exists.</summary>
		///<returns>The <paramref name="target"/> instance, for chaining.</returns>
		public static T ReadJsonFile<T>(this T target, string jsonPath) where T : SettingsBase<T> {
			if (!File.Exists(jsonPath))
				return target;

			using (var reader = new JsonTextReader(File.OpenText(jsonPath)))
				return target.ReadJson(JObject.Load(reader));
		}
		///<summary>Creates or updates a JSON file from a <see cref="SettingsBase{T}"/> instance.</summary>
		///<returns>The <paramref name="source"/> instance, for chaining.</returns>
		public static T WriteJsonFile<T>(this T source, string jsonPath) where T : SettingsBase<T> {
			using (var stream = File.Open(jsonPath, FileMode.OpenOrCreate)) {
				var encoding = Encoding.UTF8;
				JObject json;
				if (stream.Length == 0)
					json = new JObject();
				else {
					var reader = new StreamReader(stream);  // Do not dipose this, to avoid closing the stream.
					encoding = reader.CurrentEncoding ?? encoding;
					json = JObject.Load(new JsonTextReader(reader) { CloseInput = false });
					stream.SetLength(0);
				}
				source.UpdateJson(json);

				using (var writer = new JsonTextWriter(new StreamWriter(stream, encoding)))
					json.WriteTo(writer);
			}
			return source;
		}
	}
}
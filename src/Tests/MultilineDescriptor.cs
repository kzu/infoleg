using Spectre.Console;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace Clarius.OpenLaw;

public class YamlListConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => typeof(List<object?>).IsAssignableFrom(type);
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) => throw new NotImplementedException();
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value == null)
            return;

        var list = (List<object?>)value;
        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
        foreach (var item in list)
        {
            if (item == null)
                continue;

            if (item is string str)
            {
                if (str.Contains('\n'))
                    emitter.Emit(new Scalar(null, null, str, ScalarStyle.Literal, true, false));
                else
                    emitter.Emit(new Scalar(str));
            }
            else
            {
                serializer.Invoke(item);
            }
        }
        emitter.Emit(new SequenceEnd());
    }
}

public class YamlDictionaryConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => typeof(Dictionary<string, object?>).IsAssignableFrom(type);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) => throw new NotImplementedException();

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value == null)
            return;

        var dictionary = (Dictionary<string, object?>)value;
        emitter.Emit(new MappingStart());

        foreach (var kvp in dictionary)
        {
            if (kvp.Value == null)
                continue;

            emitter.Emit(new Scalar(kvp.Key));
            if (kvp.Value is string str)
            {
                if (str.Contains('\n'))
                    emitter.Emit(new Scalar(null, null, str, ScalarStyle.Literal, true, false));
                else
                    emitter.Emit(new Scalar(str));
            }
            else
            {
                serializer.Invoke(kvp.Value);
            }
        }

        emitter.Emit(new MappingEnd());
    }
}
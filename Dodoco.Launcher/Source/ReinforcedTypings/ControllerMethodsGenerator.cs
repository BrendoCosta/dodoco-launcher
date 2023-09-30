using System.Reflection;
using Reinforced.Typings;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Generators;

namespace Dodoco.Application.ReinforcedTypings {

    public class ControllerMethodsGenerator : MethodCodeGenerator {

        public override RtFunction GenerateNode(MethodInfo element, RtFunction result, TypeResolver resolver) {

            result = base.GenerateNode(element, result, resolver);

            string parameters = string.Join(", ", element.GetParameters().Select(c => c.Name));
            string fullMethodPath = $"{element.DeclaringType?.FullName}.{element.Name}";

            result.IsAsync = true;
            result.Body = new RtRaw($"return await RpcClient.GetInstance().CallAsync<{result.ReturnType.ToString()}>(\"{fullMethodPath}\", [{parameters}]);");
            result.ReturnType = new RtSimpleTypeName("Promise", result.ReturnType);

            return result;

        }

    }

}
using Reinforced.Typings;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Generators;

namespace Dodoco.Application.ReinforcedTypings {

    public class ControllerClassGenerator : ClassCodeGenerator {

        public override RtClass GenerateNode(Type element, RtClass result, TypeResolver resolver) {

            result = base.GenerateNode(element, result, resolver);

            result.Members.Add(new RtConstructor {
                
                AccessModifier =  AccessModifier.Private
                
            });
            
            result.Members.Add(new RtField {
                
                AccessModifier =  AccessModifier.Private,
                IsStatic = true,
                Identifier = new RtIdentifier("instance"),
                Type = new RtSimpleTypeName("Nullable", result.Name),
                InitializationExpression = "null"

            });

            result.Members.Add(new RtFunction {

                AccessModifier =  AccessModifier.Public,
                IsStatic = true,
                Identifier = new RtIdentifier("GetControllerInstance"),
                ReturnType = result.Name,
                Body = new RtRaw($"if ({result.Name.ToString()}.instance == null) {result.Name.ToString()}.instance = new {result.Name.ToString()}(); return {result.Name.ToString()}.instance;")

            });

            return result;

        }

    }

}
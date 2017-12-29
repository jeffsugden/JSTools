using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JS.Tools.Testing
{
    public class DataBuilder<TObject>
    {
        private readonly Dictionary<string, object> _properties;
        
        public DataBuilder(Expression<Func<TObject>> construct, int test)
        {
            var expression = (LambdaExpression)construct.Body;
            //expression.Body.NodeType == ExpressionType.
        }

        public void Setup()
        {
            // read the constutor parameters
            var ctors = typeof(TObject).GetConstructors();
            // assuming class A has only one constructor
            var ctor = ctors[0];
            foreach (var param in ctor.GetParameters())
            {
                Console.WriteLine(string.Format(
                    "Param {0} is named {1} and is of type {2}",
                    param.Position, param.Name, param.ParameterType));
            }
        }

    /// <summary>
    /// Sets Read-Only Properties by matching up to names on the constructor defined to use in this builder
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="selector"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public DataBuilder<TObject> With<TResult>(Expression<Func<TObject, TResult>> selector, TResult value)
        {
            var expression = (MemberExpression)selector.Body;
            //if (expression.NodeType == ExpressionType.Lambda) throw new Exception();
            string name = expression.Member.Name;
            if (_properties.ContainsKey(name))
            {
                _properties[name] = value;
            }
            else
            {
                _properties.Add(name, value);
            }
            return this;
        }

        public TObject Build()
        {
            return default(TObject);
        }
    }    
}

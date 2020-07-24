using System.Reflection;
using System.Linq.Expressions;

namespace T.Common
{
    public class EVisitor : ExpressionVisitor
    {
        public new Expression VisitMember(MemberExpression me)
        {
            var expression = Visit(me.Expression);
            
            if (expression is ConstantExpression)
            {
                var member = me.Member;
                                
                object value;

                object container = ((ConstantExpression)expression).Value;
                
                if (member is FieldInfo)
                {
                    value = ((FieldInfo)member).GetValue(container);
                    
                    return Expression.Constant(value);
                }
                if (member is PropertyInfo)
                {
                    value = ((PropertyInfo)member).GetValue(container, null);
                    return Expression.Constant(value);
                }
            }
            else if (expression is MemberExpression)
            {
                return VisitMember((MemberExpression)expression);
            }

            return base.VisitMember(me);
        }

        private Expression GetExpression(MemberInfo member, ConstantExpression constant)
        {
                object value = null;

                object container = constant.Value;

                if (member is FieldInfo)
                {
                    value = ((FieldInfo)member).GetValue(container);
                }
                if (member is PropertyInfo)
                {
                    value = ((PropertyInfo)member).GetValue(container, null);
                }

            if (value.HasValue() && value is ConstantExpression)
                return GetExpression(member, (ConstantExpression)value);

            return Expression.Constant(value);
        }
    }
}

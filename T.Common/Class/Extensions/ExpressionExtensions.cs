using System.Linq.Expressions;

namespace T.Common
{
    public static class ExpressionExtensions
    {
        public static Expression VisitMember(this MemberExpression member)
        {
            EVisitor ev = new EVisitor();
            return ev.VisitMember(member);
        }
    }
}

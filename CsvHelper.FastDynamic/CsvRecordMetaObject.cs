﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace CsvHelper.FastDynamic
{
    // From https://github.com/StackExchange/Dapper/blob/master/Dapper/SqlMapper.DapperRowMetaObject.cs
    internal sealed class CsvRecordMetaObject : DynamicMetaObject
    {
        private static readonly MethodInfo GetValueMethod = typeof(IDictionary<string, object>).GetProperty("Item").GetGetMethod();
        private static readonly MethodInfo SetValueMethod = typeof(CsvRecord).GetMethod(nameof(CsvRecord.SetValue), new[] { typeof(string), typeof(object) });

        public CsvRecordMetaObject(Expression expression, BindingRestrictions restrictions, object value)
            : base(expression, restrictions, value)
        {
        }

        private DynamicMetaObject CallMethod(MethodInfo method, Expression[] parameters)
        {
            var callMethod = new DynamicMetaObject(
                Expression.Call(Expression.Convert(Expression, LimitType), method, parameters),
                BindingRestrictions.GetTypeRestriction(Expression, LimitType)
            );

            return callMethod;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var parameters = new Expression[]
            {
                Expression.Constant(binder.Name)
            };

            return CallMethod(GetValueMethod, parameters);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            var parameters = new Expression[]
            {
                Expression.Constant(binder.Name)
            };

            return CallMethod(GetValueMethod, parameters);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            var parameters = new Expression[]
            {
                Expression.Constant(binder.Name),
                value.Expression
            };

            return CallMethod(SetValueMethod, parameters);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return HasValue && Value is IDictionary<string, object> lookup ? lookup.Keys : Array.Empty<string>();
        }
    }
}

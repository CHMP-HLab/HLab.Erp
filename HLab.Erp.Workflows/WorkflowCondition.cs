﻿using System;
using System.Collections.Generic;

namespace HLab.Erp.Workflows
{
    public class WorkflowCondition<T> 
        where T : IWorkflow
    {
        private WorkflowCondition<T> _next = null;
        private readonly Func<T, WorkflowConditionResult> _condition;
        private Func<T, IEnumerable<string>> _getMessage;
        private Func<T, IEnumerable<string>> _getHighlights;

        public WorkflowCondition(Func<T, WorkflowConditionResult> condition)
        {
            _condition = condition;
        }

        public void SetMessage(Func<T, IEnumerable<string>> getMessage) => _getMessage = getMessage;
        public void SetHighlights(Func<T, IEnumerable<string>> getHighlights) => _getHighlights = getHighlights;
        public void SetNext(WorkflowCondition<T> next) => _next = next;

        public bool ShowActionWhenFalse => _getMessage != null;
        public IEnumerable<string> GetMessage(T workflow)
        {
            if (_getMessage != null  && CheckThis(workflow) == WorkflowConditionResult.Failed)
            {
                var msg = _getMessage(workflow);
                foreach (var m in msg) yield return m;
            }

            if (_next != null)
            {
                var msg = _next.GetMessage(workflow);
                foreach (var m in msg) yield return m;

            }
        }
        public IEnumerable<string> GetHighlights(T workflow)
        {
            if (_getHighlights != null  && CheckThis(workflow) == WorkflowConditionResult.Failed)
            {
                var fields = _getHighlights(workflow);
                foreach (var m in fields) yield return m;
            }

            if (_next != null)
            {
                var fields = _next.GetHighlights(workflow);
                foreach (var m in fields) yield return m;
            }
        }

        public WorkflowConditionResult CheckThis(T workflow) => _condition?.Invoke(workflow) ?? WorkflowConditionResult.Passed;

        public WorkflowConditionResult CheckAll(T workflow)
        {
            var nextResult = _next?.CheckAll(workflow) ?? WorkflowConditionResult.Passed;
            if (nextResult == WorkflowConditionResult.Hidden) return WorkflowConditionResult.Hidden;

            var result = _condition?.Invoke(workflow) ?? WorkflowConditionResult.Passed;
            if (result == WorkflowConditionResult.Hidden) return WorkflowConditionResult.Hidden;
            if(nextResult == WorkflowConditionResult.Failed) return WorkflowConditionResult.Failed;

            return result;
        }
    }
}

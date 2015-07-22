/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace IronPython.EditorExtensions
{
    public enum ValidationErrorSeverity
    {
        Message,
        Warning,
        Error
    }

    public enum ValidationErrorType
    {
        Syntactic,
        Semantic
    }

    public class ValidationError
    {
        public string Description { get; private set; }
        public Span Span { get; private set; }
        public ValidationErrorSeverity Severity { get; private set; }
        public ValidationErrorType Type { get; private set; }

        public ValidationError(Span span, string description)
        {
            Span = span;
            Description = description;
            Severity = ValidationErrorSeverity.Error;
            Type = ValidationErrorType.Syntactic;
        }

        public ValidationError(Span span, string description, ValidationErrorSeverity severity, ValidationErrorType type)
            : this(span, description)
        {
            Severity = severity;
            Type = type;
        }
    }
}
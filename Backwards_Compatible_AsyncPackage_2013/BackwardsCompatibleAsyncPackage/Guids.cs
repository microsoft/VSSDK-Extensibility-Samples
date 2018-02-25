// Guids.cs
// MUST match guids.h
using System;

namespace Company.AsyncPackageTest
{
    static class GuidList
    {
        public const string guidAsyncPackageTestPkgString = "1279aa9a-dc90-4c06-86d0-8e4924b0be3d";
        public const string guidAsyncPackageTestCmdSetString = "9d9aecb9-f721-4cea-8e6d-af7724603bc0";

        public static readonly Guid guidAsyncPackageTestCmdSet = new Guid(guidAsyncPackageTestCmdSetString);
    };
}
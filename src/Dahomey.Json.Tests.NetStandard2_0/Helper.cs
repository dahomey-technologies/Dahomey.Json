﻿using Xunit;
using System;
using System.Collections;
using System.Text.Json;

namespace Dahomey.Json.Tests
{
    public static class Helper
    {
        public static T Read<T>(string json, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }

        public static void TestRead<T>(string json, T expectedValue, JsonSerializerOptions options, Type expectedExceptionType = null)
        {
            if (expectedExceptionType != null)
            {
                bool exceptionCatched = false;

                try
                {
                    Read<T>(json, options);
                }
                catch (Exception ex)
                {
                    Assert.IsType(expectedExceptionType, ex);
                    exceptionCatched = true;
                }

                Assert.True(exceptionCatched, $"Expected exception {expectedExceptionType}");
            }
            else
            {
                T actualValue = Read<T>(json, options);
                if (actualValue is ICollection actualCollection)
                {
                    Assert.Equal((ICollection)expectedValue, actualCollection);
                }
                else
                {
                    Assert.Equal(expectedValue, actualValue);
                }
            }
        }

        public static void TestRead<T>(string json, JsonSerializerOptions options, Type expectedExceptionType)
        {
            if (expectedExceptionType != null)
            {
                bool exceptionCatched = false;

                try
                {
                    Read<T>(json, options);
                }
                catch (Exception ex)
                {
                    Assert.IsType(expectedExceptionType, ex);
                    exceptionCatched = true;
                }

                Assert.True(exceptionCatched, $"Expected exception {expectedExceptionType}");
            }
            else
            {
                Read<T>(json, options);
            }
        }

        public static string Write<T>(T value, JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize<T>(value, options);
        }

        public static void TestWrite<T>(T value, string json, JsonSerializerOptions options, Type expectedExceptionType = null)
        {
            if (expectedExceptionType != null)
            {
                bool exceptionCatched = false;

                try
                {
                    Write(value, options);
                }
                catch (Exception ex)
                {
                    Assert.IsType(expectedExceptionType, ex);
                    exceptionCatched = true;
                }

                Assert.True(exceptionCatched, $"Expected exception {expectedExceptionType}");
            }
            else
            {
                Assert.Equal(json, Write(value, options));
            }
        }

        public static void TestWrite<T>(T value, JsonSerializerOptions options, Type expectedExceptionType)
        {
            if (expectedExceptionType != null)
            {
                bool exceptionCatched = false;

                try
                {
                    Write(value, options);
                }
                catch (Exception ex)
                {
                    Assert.IsType(expectedExceptionType, ex);
                    exceptionCatched = true;
                }

                Assert.True(exceptionCatched, $"Expected exception {expectedExceptionType}");
            }
            else
            {
                 Write(value, options);
            }
        }
    }
}

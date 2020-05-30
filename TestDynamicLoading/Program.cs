using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Resolvers;

namespace TestDynamicLoading
{
    class Program
    {
        private static string NonReferencedLibraryRelativePath = @"..\..\..\..\NonReferencedLibrary\bin\Debug\netcoreapp3.1\NonReferencedLibrary.dll";
        private static string NonReferencedLibrary2RelativePath = @"..\..\..\..\NonReferencedLibrary2\bin\Debug\netcoreapp3.1\NonReferencedLibrary2.dll";
        private static string NonReferencedLibrary3RelativePath = @"..\..\..\..\NonReferencedLibrary3\bin\Debug\netcoreapp3.1\NonReferencedLibrary3.dll";
        private static string ThisAssemblyLocation;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting dynamic type load testing");
            ThisAssemblyLocation = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            GetTypeByPreloading();
            GetTypeByUsingAssemblyResolve();
        }

        /// <summary>
        /// This shows that GetType will fail without preloading by using Assembly.LoadFrom
        /// Using Assembly.LoadFile cannot be used because it will load the assembly into a private AssemblyLoadContext
        /// </summary>
        static void GetTypeByPreloading()
        {
            Console.WriteLine("Testing getting a type by preloading the assembly");
            Type dynamicType = null;
            var typeName = "NonReferencedLibrary.Class1, NonReferencedLibrary, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null";
            var loadLocation = Path.Combine(ThisAssemblyLocation, NonReferencedLibraryRelativePath);

            // First try getting the type without pre-loading
            // This will fail
            dynamicType = Type.GetType(typeName);
            Debug.Assert(dynamicType == null, "Type should not have loaded");
            Console.WriteLine("Expected: loading type failed because the assembly was not preloaded");


            // Now preload the type by using Assembly.LoadFile
            // This will also fail because the type will be loaded into an assembly load context
            var loadFileAssembly = Assembly.LoadFile(loadLocation);
            Debug.Assert(loadFileAssembly != null, "Failed to load assembly");
            dynamicType = Type.GetType(typeName);
            Debug.Assert(dynamicType == null, "Type should not have loaded");
            Console.WriteLine("Expected: loading type failed because the assembly was loaded with Assembly.LoadFile");

            // Now preload the type by using Assembly.LoadFrom
            // Now preload the type by using Assembly.LoadFile
            // This will also fail because the type will be loaded into an assembly load context
            var loadFromAssembly = Assembly.LoadFrom(loadLocation);
            Debug.Assert(loadFromAssembly != null, "Failed to load assembly");
            dynamicType = Type.GetType(typeName);
            Debug.Assert(dynamicType != null, "Failed to load type");

            Console.WriteLine("Successfully get type by preloading using Assembly.LoadFrom");
        }

        static void GetTypeByUsingAssemblyResolve()
        {
            Console.WriteLine();
            Console.WriteLine("Testing getting a type by registering an assembly resolve handler");

            Type dynamicType = null;
            var typeName = "NonReferencedLibrary2.Class1, NonReferencedLibrary2, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null";
            // First try getting the type without pre-loading
            // This will fail
            dynamicType = Type.GetType(typeName);
            Debug.Assert(dynamicType == null, "Type should not have loaded");
            Console.WriteLine("Expected: loading type failed because the assembly was not preloaded");

            // Register an assembly resolve handler so we can dynamically look for any load types
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            // Load the type again. It should not succeed because the assembly load handler will find the assembly for the runtime.
            dynamicType = Type.GetType(typeName);
            Debug.Assert(dynamicType != null, "Type should have loaded");
            Console.WriteLine("Successfully loaded type because the assembly resolve handler found the assembly");
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            Console.WriteLine($"AssemblyResolve called for {assemblyName}");

            // We are hardcoding the assembly to resolve
            // In a real world use case the passed in assembly named could be searched for in a set of standard locations
            // and next to the executable.
            var loadLocation = Path.Combine(ThisAssemblyLocation, NonReferencedLibrary2RelativePath);
            var assembly = Assembly.LoadFrom(loadLocation);
            Debug.Assert(assembly != null, "Failed to load assembly");

            return assembly;
        }
    }
}

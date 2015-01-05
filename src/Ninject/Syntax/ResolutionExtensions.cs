#region License
// 
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2007-2010, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 
#endregion
#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Ninject.Activation;
using Ninject.Infrastructure;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Syntax;
#endregion

namespace Ninject
{
    /// <summary>
    /// Extensions that enhance resolution of services.
    /// </summary>
    public static class ResolutionExtensions
    {
        /// <summary>
        /// Gets an instance of the specified service.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        [NotNull]
        public static T Get<T>([NotNull] this IResolutionRoot root, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, typeof(T), null, parameters, false, true).Cast<T>().Single();
        }

        /// <summary>
        /// Gets an instance of the specified service by using the first binding with the specified name.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="name">The name of the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        [NotNull]
        public static T Get<T>([NotNull] this IResolutionRoot root, [NotNull] string name, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, typeof(T), b => b.Name == name, parameters, false, true).Cast<T>().Single();
        }

        /// <summary>
        /// Gets an instance of the specified service by using the first binding that matches the specified constraint.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="constraint">The constraint to apply to the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        [NotNull]
        public static T Get<T>([NotNull] this IResolutionRoot root, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, typeof(T), constraint, parameters, false, true).Cast<T>().Single();
        }

        /// <summary>
        /// Tries to get an instance of the specified service.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service, or <see langword="null"/> if no implementation was available.</returns>
        [CanBeNull]
        public static T TryGet<T>([NotNull] this IResolutionRoot root, [NotNull] params IParameter[] parameters)
        {
            return TryGet(GetResolutionIterator(root, typeof(T), null, parameters, true, true).Cast<T>());
        }

        /// <summary>
        /// Tries to get an instance of the specified service by using the first binding with the specified name.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="name">The name of the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service, or <see langword="null"/> if no implementation was available.</returns>
        [CanBeNull]
        public static T TryGet<T>([NotNull] this IResolutionRoot root, [NotNull] string name, [NotNull] params IParameter[] parameters)
        {
            return TryGet(GetResolutionIterator(root, typeof(T), b => b.Name == name, parameters, true, true).Cast<T>());
        }

        /// <summary>
        /// Tries to get an instance of the specified service by using the first binding that matches the specified constraint.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="constraint">The constraint to apply to the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service, or <see langword="null"/> if no implementation was available.</returns>
        [CanBeNull]
        public static T TryGet<T>([NotNull] this IResolutionRoot root, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] params IParameter[] parameters)
        {
            return TryGet(GetResolutionIterator(root, typeof(T), constraint, parameters, true, true).Cast<T>());
        }

        /// <summary>
        /// Tries to get an instance of the specified service.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service, or <see langword="null"/> if no implementation was available.</returns>
        [CanBeNull]
        public static T TryGetAndThrowOnInvalidBinding<T>([NotNull] this IResolutionRoot root, [NotNull] params IParameter[] parameters)
        {
            return DoTryGetAndThrowOnInvalidBinding<T>(root, null, parameters);
        }

        /// <summary>
        /// Tries to get an instance of the specified service by using the first binding with the specified name.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="name">The name of the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service, or <see langword="null"/> if no implementation was available.</returns>
        [CanBeNull]
        public static T TryGetAndThrowOnInvalidBinding<T>([NotNull] this IResolutionRoot root, [NotNull] string name, [NotNull] params IParameter[] parameters)
        {
            return DoTryGetAndThrowOnInvalidBinding<T>(root, b => b.Name == name, parameters);
        }

        /// <summary>
        /// Tries to get an instance of the specified service by using the first binding that matches the specified constraint.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="constraint">The constraint to apply to the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service, or <see langword="null"/> if no implementation was available.</returns>
        [CanBeNull]
        public static T TryGetAndThrowOnInvalidBinding<T>([NotNull] this IResolutionRoot root, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] params IParameter[] parameters)
        {
            return DoTryGetAndThrowOnInvalidBinding<T>(root, constraint, parameters);
        }

        /// <summary>
        /// Gets all available instances of the specified service.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>A series of instances of the service.</returns>
        [NotNull]
        public static IEnumerable<T> GetAll<T>([NotNull] this IResolutionRoot root, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, typeof(T), null, parameters, true, false).Cast<T>();
        }

        /// <summary>
        /// Gets all instances of the specified service using bindings registered with the specified name.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="name">The name of the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>A series of instances of the service.</returns>
        [NotNull]
        public static IEnumerable<T> GetAll<T>([NotNull] this IResolutionRoot root, [NotNull] string name, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, typeof(T), b => b.Name == name, parameters, true, false).Cast<T>();
        }

        /// <summary>
        /// Gets all instances of the specified service by using the bindings that match the specified constraint.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="constraint">The constraint to apply to the bindings.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>A series of instances of the service.</returns>
        [NotNull]
        public static IEnumerable<T> GetAll<T>([NotNull] this IResolutionRoot root, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, typeof(T), constraint, parameters, true, false).Cast<T>();
        }

        /// <summary>
        /// Gets an instance of the specified service.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        [NotNull]
        public static object Get([NotNull] this IResolutionRoot root, [NotNull] Type service, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, service, null, parameters, false, true).Single();
        }

        /// <summary>
        /// Gets an instance of the specified service by using the first binding with the specified name.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="name">The name of the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        [NotNull]
        public static object Get([NotNull]this IResolutionRoot root, [NotNull]Type service, [NotNull]string name, [NotNull]params IParameter[] parameters)
        {
            return GetResolutionIterator(root, service, b => b.Name == name, parameters, false, true).Single();
        }

        /// <summary>
        /// Gets an instance of the specified service by using the first binding that matches the specified constraint.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="constraint">The constraint to apply to the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        [NotNull]
        public static object Get([NotNull] this IResolutionRoot root, [NotNull] Type service, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, service, constraint, parameters, false, true).Single();
        }

        /// <summary>
        /// Tries to get an instance of the specified service.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service, or <see langword="null"/> if no implementation was available.</returns>
        [CanBeNull]
        public static object TryGet([NotNull] this IResolutionRoot root, [NotNull] Type service, [NotNull] params IParameter[] parameters)
        {
            return TryGet(GetResolutionIterator(root, service, null, parameters, true, true));
        }

        /// <summary>
        /// Tries to get an instance of the specified service by using the first binding with the specified name.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="name">The name of the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service, or <see langword="null"/> if no implementation was available.</returns>
        [CanBeNull]
        public static object TryGet([NotNull] this IResolutionRoot root, [NotNull] Type service, [NotNull] string name, [NotNull] params IParameter[] parameters)
        {
            return TryGet(GetResolutionIterator(root, service, b => b.Name == name, parameters, true, false));
        }

        /// <summary>
        /// Tries to get an instance of the specified service by using the first binding that matches the specified constraint.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="constraint">The constraint to apply to the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service, or <see langword="null"/> if no implementation was available.</returns>
        [CanBeNull]
        public static object TryGet([NotNull] this IResolutionRoot root, [NotNull] Type service, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] params IParameter[] parameters)
        {
            return TryGet(GetResolutionIterator(root, service, constraint, parameters, true, false));
        }

        /// <summary>
        /// Gets all available instances of the specified service.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>A series of instances of the service.</returns>
        [NotNull]
        public static IEnumerable<object> GetAll([NotNull] this IResolutionRoot root, [NotNull] Type service, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, service, null, parameters, true, false);
        }

        /// <summary>
        /// Gets all instances of the specified service using bindings registered with the specified name.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="name">The name of the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>A series of instances of the service.</returns>
        [NotNull]
        public static IEnumerable<object> GetAll([NotNull] this IResolutionRoot root, [NotNull] Type service, [NotNull] string name, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, service, b => b.Name == name, parameters, true, false);
        }

        /// <summary>
        /// Gets all instances of the specified service by using the bindings that match the specified constraint.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="constraint">The constraint to apply to the bindings.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>A series of instances of the service.</returns>
        [NotNull]
        public static IEnumerable<object> GetAll([NotNull] this IResolutionRoot root, [NotNull] Type service, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] params IParameter[] parameters)
        {
            return GetResolutionIterator(root, service, constraint, parameters, true, false);
        }

        /// <summary>
        /// Evaluates if an instance of the specified service can be resolved.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        public static bool CanResolve<T>([NotNull] this IResolutionRoot root, [NotNull] params IParameter[] parameters)
        {
            return CanResolve(root, typeof(T), null, parameters, false, true);
        }

        /// <summary>
        /// Evaluates if  an instance of the specified service by using the first binding with the specified name can be resolved.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="name">The name of the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        public static bool CanResolve<T>([NotNull] this IResolutionRoot root, [NotNull] string name, [NotNull] params IParameter[] parameters)
        {
            return CanResolve(root, typeof(T), b => b.Name == name, parameters, false, true);
        }

        /// <summary>
        /// Evaluates if  an instance of the specified service by using the first binding that matches the specified constraint can be resolved.
        /// </summary>
        /// <typeparam name="T">The service to resolve.</typeparam>
        /// <param name="root">The resolution root.</param>
        /// <param name="constraint">The constraint to apply to the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        public static bool CanResolve<T>([NotNull] this IResolutionRoot root, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] params IParameter[] parameters)
        {
            return CanResolve(root, typeof(T), constraint, parameters, false, true);
        }

        /// <summary>
        /// Gets an instance of the specified service.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        [NotNull]
        public static object CanResolve([NotNull] this IResolutionRoot root, [NotNull] Type service, [NotNull] params IParameter[] parameters)
        {
            return CanResolve(root, service, null, parameters, false, true);
        }

        /// <summary>
        /// Gets an instance of the specified service by using the first binding with the specified name.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="name">The name of the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        [NotNull]
        public static object CanResolve([NotNull] this IResolutionRoot root, [NotNull] Type service, [NotNull] string name, [NotNull] params IParameter[] parameters)
        {
            return CanResolve(root, service, b => b.Name == name, parameters, false, true);
        }

        /// <summary>
        /// Gets an instance of the specified service by using the first binding that matches the specified constraint.
        /// </summary>
        /// <param name="root">The resolution root.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="constraint">The constraint to apply to the binding.</param>
        /// <param name="parameters">The parameters to pass to the request.</param>
        /// <returns>An instance of the service.</returns>
        [NotNull]
        public static object CanResolve([NotNull] this IResolutionRoot root, [NotNull] Type service, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] params IParameter[] parameters)
        {
            return CanResolve(root, service, constraint, parameters, false, true);
        }

        private static bool CanResolve([NotNull] IResolutionRoot root, [NotNull] Type service, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] IEnumerable<IParameter> parameters, bool isOptional, bool isUnique)
        {
            Ensure.ArgumentNotNull(root, "root");
            Ensure.ArgumentNotNull(service, "service");
            Ensure.ArgumentNotNull(parameters, "parameters");

            IRequest request = root.CreateRequest(service, constraint, parameters, isOptional, isUnique);
            return root.CanResolve(request);
        }

        [NotNull]
        private static IEnumerable<object> GetResolutionIterator([NotNull] IResolutionRoot root, [NotNull] Type service, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] IEnumerable<IParameter> parameters, bool isOptional, bool isUnique)
        {
            Ensure.ArgumentNotNull(root, "root");
            Ensure.ArgumentNotNull(service, "service");
            Ensure.ArgumentNotNull(parameters, "parameters");

            IRequest request = root.CreateRequest(service, constraint, parameters, isOptional, isUnique);
            return root.Resolve(request);
        }
        
        [NotNull]
        private static IEnumerable<object> GetResolutionIterator([NotNull] IResolutionRoot root, [NotNull] Type service, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] IEnumerable<IParameter> parameters, bool isOptional, bool isUnique, bool forceUnique)
        {
            Ensure.ArgumentNotNull(root, "root");
            Ensure.ArgumentNotNull(service, "service");
            Ensure.ArgumentNotNull(parameters, "parameters");

            IRequest request = root.CreateRequest(service, constraint, parameters, isOptional, isUnique);
            request.ForceUnique = forceUnique;
            return root.Resolve(request);
        }

        [CanBeNull]
        private static T TryGet<T>([NotNull] IEnumerable<T> iterator)
        {
            try
            {
                return iterator.SingleOrDefault();
            }
            catch (ActivationException)
            {
                return default(T);
            }
        }

        [CanBeNull]
        private static T DoTryGetAndThrowOnInvalidBinding<T>([NotNull] IResolutionRoot root, [CanBeNull] Func<IBindingMetadata, bool> constraint, [NotNull] IEnumerable<IParameter> parameters)
        {
            return GetResolutionIterator(root, typeof(T), constraint, parameters, true, true, true).Cast<T>().SingleOrDefault();
        }
    }
}

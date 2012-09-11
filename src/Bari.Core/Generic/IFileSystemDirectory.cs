﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bari.Core.Generic
{
    /// <summary>
    /// Abstraction of a file system directory
    /// </summary>
    [ContractClass(typeof(IFileSystemDirectoryContracts))]
    public interface IFileSystemDirectory
    {
        /// <summary>
        /// Enumerates all the files within the directory by their names
        /// </summary>
        IEnumerable<string> Files { get; }
            
        /// <summary>
        /// Enumerates all the child directories of the directory by their names
        /// 
        /// <para>Use <see cref="GetChildDirectory"/> to get any of these children.</para>
        /// </summary>
        IEnumerable<string> ChildDirectories { get; }

        /// <summary>
        /// Gets interface for a given child directory
        /// </summary>
        /// <param name="name">Name of the child directory</param>
        /// <returns>Returns either a directory abstraction or <c>null</c> if it does not exists.</returns>
        IFileSystemDirectory GetChildDirectory(string name);

        /// <summary>
        /// Gets the relative path from this directory to another directory (in any depth)
        /// 
        /// <para>If the given argument is not a child of this directory, an <see cref="ArgumentException"/>will
        /// be thrown.</para>
        /// </summary>
        /// <param name="childDirectory">The child directory to get path to</param>
        /// <returns>Returns the path</returns>
        string GetRelativePath(IFileSystemDirectory childDirectory);
    }

    [ContractClassFor(typeof(IFileSystemDirectory))]
    public abstract class IFileSystemDirectoryContracts: IFileSystemDirectory
    {
        /// <summary>
        /// Enumerates all the files within the directory by their names
        /// </summary>
        public IEnumerable<string> Files
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<string>>(), name => name  != null));

                return null; // dummy value
            }
        }

        /// <summary>
        /// Enumerates all the child directories of the directory by their names
        /// 
        /// <para>Use <see cref="IFileSystemDirectory.GetChildDirectory"/> to get any of these children.</para>
        /// </summary>
        public IEnumerable<string> ChildDirectories
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<string>>(), name => name  != null));

                return null; // dummy value
            }
        }

        /// <summary>
        /// Gets interface for a given child directory
        /// </summary>
        /// <param name="name">Name of the child directory</param>
        /// <returns>Returns either a directory abstraction or <c>null</c> if it does not exists.</returns>
        public IFileSystemDirectory GetChildDirectory(string name)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(name));
            Contract.Ensures(ChildDirectories.Contains(name) && Contract.Result<IFileSystemDirectory>() != null ||
                             !ChildDirectories.Contains(name) && Contract.Result<IFileSystemDirectory>() == null);
            
            return null; // dummy value
        }

        /// <summary>
        /// Gets the relative path from this directory to another directory (in any depth)
        /// 
        /// <para>If the given argument is not a child of this directory, an <see cref="ArgumentException"/>will
        /// be thrown.</para>
        /// </summary>
        /// <param name="childDirectory">The child directory to get path to</param>
        /// <returns>Returns the path</returns>
        public string GetRelativePath(IFileSystemDirectory childDirectory)
        {
            Contract.Requires(childDirectory != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return null; // dummy value
        }
    }
}
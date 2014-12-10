﻿using System;
using System.Collections.Generic;
using System.IO;
using Bari.Core.Build.Dependencies;
using Bari.Core.Generic;
using Bari.Core.Model;
using Bari.Core.UI;

namespace Bari.Core.Build
{
    [FallbackToCache]
    public class FileReferenceBuilder: IReferenceBuilder
    {
        private readonly IFileSystemDirectory targetRoot;
        private readonly IUserOutput output;
        private Reference reference;

        public FileReferenceBuilder([TargetRoot] IFileSystemDirectory targetRoot, IUserOutput output)
        {
            this.targetRoot = targetRoot;
            this.output = output;
        }

        /// <summary>
        /// Dependencies required for running this builder
        /// </summary>
        public IDependencies Dependencies
        {
            get
            {
                return new NoDependencies(); // TODO: dependency fingerprint
            }
        }

        /// <summary>
        /// Gets an unique identifier which can be used to identify cached results
        /// </summary>
        public string Uid
        {
            get
            {
                return String.Format("{0}.{1}", reference.Uri.Host, reference.Uri.PathAndQuery
                    .Replace('/', '_'))
                    .Replace("*.*", "___allfiles___");
            }
        }

        /// <summary>
        /// Prepares a builder to be ran in a given build context.
        /// 
        /// <para>This is the place where a builder can add additional dependencies.</para>
        /// </summary>
        /// <param name="context">The current build context</param>
        public void AddToContext(IBuildContext context)
        {
            context.AddBuilder(this, new IBuilder[0]);
        }

        /// <summary>
        /// Runs this builder
        /// </summary>
        /// <param name="context">Current build context</param>
        /// <returns>Returns a set of generated files, in target relative paths</returns>
        public ISet<TargetRelativePath> Run(IBuildContext context)
        {
            if (output != null)
                output.Message(String.Format("Resolving reference {0}", reference.Uri));

            var depsRoot = targetRoot.CreateDirectory("deps");
            var sourcePath = reference.Uri.OriginalString.Substring(7).Replace('/', Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
            var fileName = Path.GetFileName(sourcePath);
            
            using (var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var target = depsRoot.CreateBinaryFile(sourcePath))
            {
                StreamOperations.Copy(source, target);
            }

            return new HashSet<TargetRelativePath>(new[]
                {
                    new TargetRelativePath(targetRoot.GetRelativePath(depsRoot), fileName)
                });
        }

        /// <summary>
        /// Verifies if the builder is able to run. Can be used to fallback to cached results without getting en error.
        /// </summary>
        /// <returns>If <c>true</c>, the builder thinks it can run.</returns>
        public bool CanRun()
        {
            return true;
        }

		public Type BuilderType
		{
			get
			{
				return typeof(FileReferenceBuilder);
			}
		}

        /// <summary>
        /// Gets or sets the reference to be resolved
        /// </summary>
        public Reference Reference
        {
            get { return reference; }
            set { reference = value; }
        }

        /// <summary>
        /// If <c>false</c>, the reference builder can be ignored as an optimization
        /// </summary>
        public bool IsEffective
        {
            get { return true; }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(FileReferenceBuilder other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(reference, other.reference);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FileReferenceBuilder)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return (reference != null ? reference.GetHashCode() : 0);
        }

        public static bool operator ==(FileReferenceBuilder left, FileReferenceBuilder right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileReferenceBuilder left, FileReferenceBuilder right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("[{0}]", reference.Uri);
        }
    }
}
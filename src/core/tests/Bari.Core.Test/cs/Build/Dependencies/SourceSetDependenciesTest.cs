﻿using System;
using System.Collections.Generic;
using System.IO;
using Bari.Core.Build.Dependencies;
using Bari.Core.Build.Dependencies.Protocol;
using Bari.Core.Generic;
using Bari.Core.Model;
using Bari.Core.Test.Helper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ninject;

namespace Bari.Core.Test.Build.Dependencies
{
    [TestFixture]
    public class SourceSetDependenciesTest
    {
        private IKernel kernel;
        private TempDirectory tmp;
        private IFileSystemDirectory rootDir;
        private SourceSet sourceSet;
        private ISourceSetFingerprintFactory fingerprintFactory;

        [SetUp]
        public void SetUp()
        {
            kernel = new StandardKernel();

            tmp = new TempDirectory();
            rootDir = new LocalFileSystemDirectory(tmp);
            using (var writer = rootDir.CreateTextFile("file1"))
                writer.WriteLine("Contents of file 1");
            using (var writer = rootDir.CreateTextFile("file2"))
                writer.WriteLine("Contents of file 2");

            sourceSet = new SourceSet("test");
            sourceSet.Add(new SuiteRelativePath("file1"));
            sourceSet.Add(new SuiteRelativePath("file2"));

            RebindRootDir();

            var factoryMock = new Mock<ISourceSetFingerprintFactory>();
            factoryMock.Setup(
                f =>
                f.CreateSourceSetFingerprint(It.IsAny<IEnumerable<SuiteRelativePath>>(), It.IsAny<Func<string, bool>>(), It.IsAny<bool>()))
                       .Returns<IEnumerable<SuiteRelativePath>, Func<string, bool>, bool>(
                            (files, exclusions, fullDependency) => new SourceSetFingerprint(rootDir, files, exclusions, fullDependency));
            fingerprintFactory = factoryMock.Object;
        }

        private void RebindRootDir()
        {
            rootDir = new LocalFileSystemDirectory(tmp);
            kernel.Rebind<IFileSystemDirectory>().ToConstant(rootDir).WhenTargetHas<SuiteRootAttribute>();
        }

        [TearDown]
        public void TearDown()
        {
            tmp.Dispose();
            kernel.Dispose();
        }

        [Test]
        public void CreatesSameFingerprintForSameState()
        {
            var dep1 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var dep2 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp1 = dep1.Fingerprint;
            var fp2 = dep2.Fingerprint;

            fp1.Should().Be(fp2);
            fp2.Should().Be(fp1);
        }

        [Test]
        public void RemovingFileChangesFingerprint()
        {
            var dep1 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp1 = dep1.Fingerprint;

            File.Delete(Path.Combine(tmp, "file1"));
            sourceSet.Remove(new SuiteRelativePath("file1"));

            var dep2 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp2 = dep2.Fingerprint;

            fp1.Should().NotBe(fp2);
        }

        [Test]
        public void AddingFileChangesFingerprint()
        {
            var dep1 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp1 = dep1.Fingerprint;

            using (var writer = rootDir.CreateTextFile("file3"))
                writer.WriteLine("Contents of file 3");
            sourceSet.Add(new SuiteRelativePath("file3"));

            var dep2 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp2 = dep2.Fingerprint;

            fp1.Should().NotBe(fp2);
        }

        [Test]
        public void ModifyingFileChangesFingerprint()
        {
            var dep1 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp1 = dep1.Fingerprint;

            using (var writer = File.CreateText(Path.Combine(tmp, "file2")))
            {
                writer.WriteLine("Modified contents");
                writer.Flush();
            }

            RebindRootDir();

            var dep2 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp2 = dep2.Fingerprint;

            fp1.Should().NotBe(fp2);
        }

        [Test]
        public void AddingFileToSubdirectoryChangesFingerprint()
        {
            var dep1 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp1 = dep1.Fingerprint;

            Directory.CreateDirectory(Path.Combine(tmp, "subdir"));
            using (var writer = rootDir.GetChildDirectory("subdir").CreateTextFile("file3"))
                writer.WriteLine("Contents of file 3");
            sourceSet.Add(new SuiteRelativePath(Path.Combine("subdir", "file3")));

            var dep2 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp2 = dep2.Fingerprint;

            fp1.Should().NotBe(fp2);
        }

        [Test]
        public void ModifyingFileInSubdirectoryChangesFingerprint()
        {
            var dep1 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            
            Directory.CreateDirectory(Path.Combine(tmp, "subdir"));
            using (var writer = rootDir.GetChildDirectory("subdir").CreateTextFile("file3"))
                writer.WriteLine("Contents of file 3");
            sourceSet.Add(new SuiteRelativePath(Path.Combine("subdir", "file3")));

            var fp1 = dep1.Fingerprint;

            using (var writer = rootDir.GetChildDirectory("subdir").CreateTextFile("file3"))
                writer.WriteLine("Modified contents of file 3");

            RebindRootDir();

            var dep2 = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp2 = dep2.Fingerprint;

            fp1.Should().NotBe(fp2);
        }

        [Test]
        public void ConvertToProtocolAndBack()
        {
            var dep = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp1 = dep.Fingerprint;

            var proto = fp1.Protocol;
            var fp2 = proto.CreateFingerprint();

            fp1.Should().Be(fp2);
        }

        [Test]
        public void SerializeAndReadBack()
        {
            var registry = new DependencyFingerprintProtocolRegistry();
            registry.Register<SourceSetFingerprintProtocol>();

            var ser = new BinarySerializer(registry);
            var dep = new SourceSetDependencies(fingerprintFactory, sourceSet);
            var fp1 = dep.Fingerprint;

            byte[] data;
            using (var ms = new MemoryStream())
            {
                fp1.Save(ser, ms);
                data = ms.ToArray();
            }

            SourceSetFingerprint fp2;
            using (var ms = new MemoryStream(data))
            {
                fp2 = new SourceSetFingerprint(ser, ms);
            }            

            fp1.Should().Be(fp2);
        }
    }
}
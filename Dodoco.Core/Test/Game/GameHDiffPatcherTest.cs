namespace Dodoco.Core.Test.Game;

using Dodoco.Core.Game;
using Dodoco.Core.Util.Hash;

using NUnit.Framework;
using System.Security.Cryptography;

[TestFixture]
public class GameHDiffPatcherTest {

    [Test, Description("Patch a source file to get a new file")]
    public async Task Patch_Test() {

        string sourceFilePath = Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameHDiffPatcherTest/Patch_Test/source");
        string targetFilePath = Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameHDiffPatcherTest/Patch_Test/target");
        string diffFilePath = Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameHDiffPatcherTest/Patch_Test/diff.hdiff");
        string newFilePath = Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameHDiffPatcherTest/Patch_Test/new");
        
        GameHDiffPatcher patcher = new GameHDiffPatcher();
        await patcher.Patch(diffFilePath, sourceFilePath, newFilePath);
        
        // The new file should exist
        
        Assert.That(File.Exists(newFilePath), Is.True);
        
        /*
         * The new file's checksum should be equal to the target file's checksum.
         * In other words, the two files should be the same
        */

        Assert.That(new Hash(MD5.Create()).ComputeHash(newFilePath).ToUpper(), Is.EqualTo(new Hash(MD5.Create()).ComputeHash(targetFilePath).ToUpper()));

    }

}
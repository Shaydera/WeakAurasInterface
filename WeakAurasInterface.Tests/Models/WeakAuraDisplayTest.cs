using WeakAurasInterface.Core.Models;

namespace WeakAurasInterface.Tests.Models;

[TestFixture]
public class WeakAuraDisplayTest
{
    [Test]
    public void Constructor()
    {
        const string id = "subject";
        var subject = new WeakAuraDisplay(id);
        Assert.That(subject, Is.Not.Null);
        Assert.That(subject.Id, Is.EqualTo(id));
    }

    [Test]
    public void NodeStructure()
    {
        var rootOne = new WeakAuraDisplay("");
        var rootTwo = new WeakAuraDisplay("");
        var branchOne = new WeakAuraDisplay("") { Parent = rootOne };
        var subBranchOne = new WeakAuraDisplay("") { Parent = branchOne };
        var leafOne = new WeakAuraDisplay("") { Parent = rootOne };
        var leafTwo = new WeakAuraDisplay("") { Parent = rootOne };
        var leafThree = new WeakAuraDisplay("") { Parent = branchOne };
        var leafFour = new WeakAuraDisplay("") { Parent = subBranchOne };

        Assert.That(rootOne.Children, Is.Not.Null);
        Assert.That(rootOne.Children, Has.Count.EqualTo(3));
        Assert.That(rootOne.Children, Contains.Item(branchOne));
        Assert.That(rootOne.Children, Contains.Item(leafOne));
        Assert.That(rootOne.Children, Contains.Item(leafTwo));

        Assert.That(branchOne.Children, Is.Not.Null);
        Assert.That(branchOne.Children, Has.Count.EqualTo(2));
        Assert.That(branchOne.Children, Contains.Item(leafThree));
        Assert.That(branchOne.Children, Contains.Item(subBranchOne));

        Assert.That(subBranchOne.Children, Is.Not.Null);
        Assert.That(subBranchOne.Children, Has.Count.EqualTo(1));
        Assert.That(subBranchOne.Children, Contains.Item(leafFour));

        branchOne.Parent = null;
        Assert.That(rootOne.Children, Has.Count.EqualTo(2));
        Assert.That(rootOne.Children, Has.No.Member(branchOne));

        rootOne.RemoveChild(leafTwo);
        Assert.That(rootOne.Children, Has.Count.EqualTo(1));
        Assert.That(rootOne.Children, Contains.Item(leafOne));
        Assert.IsNull(leafTwo.Parent);

        rootOne.AppendChild(leafTwo);
        Assert.That(leafTwo.Parent, Is.EqualTo(rootOne));
        Assert.That(rootOne.Children, Has.Count.EqualTo(2));

        branchOne.Parent = rootTwo;
        Assert.That(rootTwo.Children, Has.Count.EqualTo(1));
        Assert.That(rootTwo.Children, Contains.Item(branchOne));
        Assert.That(branchOne.Children, Contains.Item(subBranchOne));
    }
}
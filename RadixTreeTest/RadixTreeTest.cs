using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using RadixTree;


namespace RadixTreeTest
{
    [TestFixture]
    public class NodeTest {

        Node<String> tree; 
    
        [SetUp]
        public void CreateTree() {
            tree = Node<string>.Root();
        }
    
        [Test]
        public void TestSearchForPartialParentAndLeafKeyWhenOverlapExists() {
            tree.Insert("abcd", "abcd");
            tree.Insert("abce", "abce");
        
            Assert.That(tree.Search("abe").Count, Is.EqualTo(0));
            Assert.That(tree.Search("abd").Count, Is.EqualTo(0));
        }
    
        [Test]
        public void TestSearchForLeafNodesWhenOverlapExists() {
            tree.Insert("abcd", "abcd");
            tree.Insert("abce", "abce");
   
            Assert.That(tree.Search("abcd").Count, Is.EqualTo(1));
            Assert.That(tree.Search("abce").Count, Is.EqualTo(1));
        }
    
        [Test]
        public void TestSearchForStringSmallerThanSharedParentWhenOverlapExists() {
            tree.Insert("abcd", "abcd");
            tree.Insert("abce", "abce");

            Assert.That(tree.Search("ab").Count, Is.EqualTo(2));
            Assert.That(tree.Search("a").Count, Is.EqualTo(2));
        }
    
        [Test]
        public void TestSearchForStringEqualToSharedParentWhenOverlapExists() {
            tree.Insert("abcd", "abcd");
            tree.Insert("abce", "abce");

            Assert.That(tree.Search("abc").Count, Is.EqualTo(2));
        }
    
        [Test]
        public void TestInsert() {
            tree.Insert("apple", "apple");
            tree.Insert("bat", "bat");
            tree.Insert("ape", "ape");
            tree.Insert("bath", "bath");
            tree.Insert("banana", "banana"); 
        
            Assert.That(tree.Find("apple"), Is.EqualTo("apple"));
            Assert.That(tree.Find("bat"), Is.EqualTo("bat"));
            Assert.That(tree.Find("ape"), Is.EqualTo("ape"));
            Assert.That(tree.Find("bath"), Is.EqualTo("bath"));
            Assert.That(tree.Find("banana"), Is.EqualTo("banana"));
        }
    
        [Test]
        public void TestInsertExistingUnrealNodeConvertsItToReal() {
            tree.Insert("applepie", "applepie");
            tree.Insert("applecrisp", "applecrisp");
    	
            Assert.That(tree.Contains("apple"), Is.False);
    	
            tree.Insert("apple", "apple");
    	
            Assert.That(tree.Contains("apple"));
        }
    
        [Test]
        public void TestDuplicatesNotAllowed() {
            var trie = Node<string>.Root();

            trie.Insert("apple", "apple");

            try {
                trie.Insert("apple", "apple2");
                Assert.Fail("Duplicate should not have been allowed");
            } catch (DuplicateKeyException e) {
                Assert.That(e.Message, Is.EqualTo("Duplicate key: 'apple'"));
            }
        }
    
        [Test]
        public void TestInsertWithRepeatingPatternsInKey() {
            tree.Insert("xbox 360", "xbox 360");
            tree.Insert("xbox", "xbox");
            tree.Insert("xbox 360 games", "xbox 360 games");
            tree.Insert("xbox games", "xbox games");
            tree.Insert("xbox xbox 360", "xbox xbox 360");
            tree.Insert("xbox xbox", "xbox xbox");
            tree.Insert("xbox 360 xbox games", "xbox 360 xbox games");
            tree.Insert("xbox games 360", "xbox games 360");
            tree.Insert("xbox 360 360", "xbox 360 360");
            tree.Insert("xbox 360 xbox 360", "xbox 360 xbox 360");
            tree.Insert("360 xbox games 360", "360 xbox games 360");
            tree.Insert("xbox xbox 361", "xbox xbox 361");
        
            Assert.That(tree.Size(), Is.EqualTo(12));
        }

        [Test]
        public void TestDeleteNodeWithNoChildren() {
            var trie = Node<string>.Root();
            trie.Insert("apple", "apple");
            Assert.That(trie.Delete("apple"));
        }

        [Test, Ignore("This contradicts with TestDeleteNodeWithMultipleChildren")]
        public void TestDeleteNodeWithOneChild() {
            var trie = new Node<String>();
            trie.Insert("apple", "apple");
            trie.Insert("applepie", "applepie");
            Assert.That(trie.Contains("apple"));
            Assert.That(trie.Delete("apple"));
            Assert.That(trie.Contains("applepie"));
            Assert.That(trie.Contains("apple"));
        }
    
        [Test]
        public void TestDeleteNodeWithMultipleChildren() {
            var trie = Node<string>.Root();
            trie.Insert("apple", "apple");
            trie.Insert("applepie", "applepie");
            trie.Insert("applecrisp", "applecrisp");
            Assert.That(trie.Delete("apple"));
            Assert.That(trie.Contains("applepie"));
            Assert.That(trie.Contains("applecrisp"));
            Assert.That(trie.Contains("apple"), Is.False);
        }
    
        [Test]
        public void TestCantDeleteSomethingThatDoesntExist() {
            var trie = new Node<String>();
            Assert.That(trie.Delete("apple"), Is.False);
        }

        [Test]
        public void TestCantDeleteSomethingThatWasAlreadyDeleted() {
            var trie = Node<string>.Root();
            trie.Insert("apple", "apple");
            trie.Delete("apple");
            Assert.That(trie.Delete("apple"), Is.False);
        }

        [Test, Ignore("Ignoring all deletes for now. But fix it later..")]
        public void TestChildrenNotAffectedWhenOneIsDeleted() {
            var trie = new Node<String>();
            trie.Insert("apple", "apple");
            trie.Insert("appleshack", "appleshack");
            trie.Insert("applepie", "applepie");
            trie.Insert("ape", "ape");
        
            trie.Delete("apple");

            Assert.That(trie.Contains("appleshack"));
            Assert.That(trie.Contains("applepie"));
            Assert.That(trie.Contains("ape"));
            Assert.That(trie.Contains("apple"), Is.False);
        }
    
        [Test]
        public void TestSiblingsNotAffectedWhenOneIsDeleted() {
            var trie = Node<string>.Root();
            trie.Insert("apple", "apple");
            trie.Insert("ball", "ball");
        
            trie.Delete("apple");

            Assert.That(trie.Contains("ball"));
        }
    
        [Test]
        public void TestCantDeleteUnrealNode() {
            var trie = Node<string>.Root();
            trie.Insert("apple", "apple");
            trie.Insert("ape", "ape");

            Assert.That(trie.Delete("ap"), Is.False);
        }
    

        [Test]
        public void TestCantFindRootNode() {
            Assert.That(tree.Find(""), Is.Null);
        }

        [Test]
        public void TestFindSimpleInsert() {
            tree.Insert("apple", "apple");
            Assert.That(tree.Find("apple"), Is.Not.Null);
        }
    
        [Test]
        public void TestContainsSimpleInsert() {
            tree.Insert("apple", "apple");
            Assert.That(tree.Contains("apple"));
        }

        [Test]
        public void TestFindChildInsert() {
            tree.Insert("apple", "apple");
            tree.Insert("ape", "ape");
            tree.Insert("appletree", "appletree");
            tree.Insert("appleshackcream", "appleshackcream");
            Assert.That(tree.Find("appletree"), Is.Not.Null);
            Assert.That(tree.Find("appleshackcream"), Is.Not.Null);
            Assert.That(tree.Contains("ape"), Is.Not.Null);
        }
    
        [Test]
        public void TestContainsChildInsert() {
            tree.Insert("apple", "apple");
            tree.Insert("ape", "ape");
            tree.Insert("appletree", "appletree");
            tree.Insert("appleshackcream", "appleshackcream");
            Assert.That(tree.Contains("appletree"));
            Assert.That(tree.Contains("appleshackcream"));
            Assert.That(tree.Contains("ape"));
        }

        [Test]
        public void TestCantFindNonexistantNode() {
            Assert.That(tree.Find("apple"), Is.Null);
        }

        [Test]
        public void TestDoesntContainNonexistantNode() {
            Assert.That(tree.Contains("apple"), Is.False);
        }
    
        [Test]
        public void TestCantFindUnrealNode() {
            tree.Insert("apple", "apple");
            tree.Insert("ape", "ape");
            Assert.That(tree.Find("ap"), Is.Null);
        }

        [Test]
        public void TestDoesntContainUnrealNode() {
            tree.Insert("apple", "apple");
            tree.Insert("ape", "ape");
            Assert.That(tree.Contains("ap"), Is.False);
        }


        [Test]
        public void TestSearchPrefixLimitGreaterThanPossibleResults() {
            tree.Insert("apple", "apple");
            tree.Insert("appleshack", "appleshack");
            tree.Insert("appleshackcream", "appleshackcream");
            tree.Insert("applepie", "applepie");
            tree.Insert("ape", "ape");

            var result = tree.Search("app");
            Assert.That(result.Count, Is.EqualTo(4));

            Assert.That(result.Contains("appleshack"));
            Assert.That(result.Contains("appleshackcream"));
            Assert.That(result.Contains("applepie"));
            Assert.That(result.Contains("apple"));
        }
    
        [Test]
        public void TestSearchPrefixLimitLessThanPossibleResults() {
            tree.Insert("apple", "apple");
            tree.Insert("appleshack", "appleshack");
            tree.Insert("appleshackcream", "appleshackcream");
            tree.Insert("applepie", "applepie");
            tree.Insert("ape", "ape");

            var result = tree.Search("appl");
            Assert.That(result.Count, Is.EqualTo(4));

            Assert.That(result.Contains("appleshack"));
            Assert.That(result.Contains("applepie"));
            Assert.That(result.Contains("apple"));
            Assert.That(result.Contains("appleshackcream"));
        }

        [Test]
        public void TestGetSize() {
            tree.Insert("apple", "apple");
            tree.Insert("appleshack", "appleshack");
            tree.Insert("appleshackcream", "appleshackcream");
            tree.Insert("applepie", "applepie");
            tree.Insert("ape", "ape");

            Assert.That(tree.Size(), Is.EqualTo(5));
        }
    
        [Test]
        public void TestDeleteReducesSize() {
            tree.Insert("apple", "apple");
            tree.Insert("appleshack", "appleshack");
        
            tree.Delete("appleshack");

            Assert.That(tree.Size(), Is.EqualTo(1));
        }

        [Test]
        public void Equality()
        {
            var one = new RadixTreeForEquality("xbox", "xbox");
            var two = new RadixTreeForEquality("xbox", "xbox");

            Assert.That(one.Equals(two));
        }
    }

    public class RadixTreeForEquality: Node<string>
    {
        public RadixTreeForEquality(string key , string value ): base(key, value)
        {
            
        }
    }
}
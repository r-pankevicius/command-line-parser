﻿
#region Using Directives

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.CommandLine.Parser.Antlr;
using System.IO;
using System.Linq;

#endregion

namespace System.CommandLine.Parser.UnitTests
{
    /// <summary>
    /// Represents a unit testing class, which contains all of the unit tests for the command line parser.
    /// </summary>
    [TestClass]
    public class AntlrParserUnitTests
    {
        #region Private Methods

        /// <summary>
        /// Lexes the specified input.
        /// </summary>
        /// <param name="input">The input that is to be lexed.</param>
        /// <returns>Returns the lexed tokens from the specified input.</returns>
        private CommandLineLexer LexInput(string input)
        {
            // Creates a new stream input for the input string
            AntlrInputStream stream = new AntlrInputStream(new StringReader(input));

            // Lexes the input and returns the lexer
            CommandLineLexer lexer = new CommandLineLexer(stream);
            return lexer;
        }

        /// <summary>
        /// Parses the specified token stream.
        /// </summary>
        /// <param name="lexer">The lexer, which lexes the token consumed by the parser.</param>
        /// <returns>Returns the parse tree generated by parsing the specified token stream.</returns>
        private IParseTree ParseTokens(CommandLineLexer lexer, out CommandLineParser parser)
        {
            // Parses the token stream
            parser = new CommandLineParser(new CommonTokenStream(lexer))
            {
                BuildParseTree = true
            };
            IParseTree parseTree = parser.commandLine();

            // Returns the generated parse tree
            return parseTree;
        }

        /// <summary>
        /// Validates the parse tree generated by the parser against an expected parse tree.
        /// </summary>
        /// <param name="parser">The parser that was used to generate the parse tree.</param>
        /// <param name="parseTree">The parse tree generated by the parser.</param>
        /// <param name="expectedParseTree">The parse tree that was expected from the parser and against which the actual parse tree is being validated.</param>
        private void ValidateParseTree(CommandLineParser parser, IParseTree parseTree, TreeNode expectedParseTree)
        {
            // Validates the current node of the parse tree
            ParserRuleContext typedParseTree = parseTree as ParserRuleContext;
            if (typedParseTree != null)
            {
                if (expectedParseTree.RuleName != null)
                    Assert.AreEqual(parser.RuleNames[typedParseTree.RuleIndex], expectedParseTree.RuleName);
                if (expectedParseTree.Content != null)
                    Assert.AreEqual(typedParseTree.GetText(), expectedParseTree.Content);
            }
            if (expectedParseTree.IsTerminalNode)
                Assert.IsInstanceOfType(parseTree, typeof(TerminalNodeImpl));

            // Checks if the amount of child nodes are the same in the parser generated parse tree and the expected parse tree
            Assert.AreEqual(parseTree.ChildCount, expectedParseTree.Children.Count());
            if (parseTree.ChildCount != expectedParseTree.Children.Count())
                return;

            // Cycles over all direct children of the current node of the parse tree and validates them recursively
            for (int i = 0; i < parseTree.ChildCount; i++)
                this.ValidateParseTree(parser, parseTree.GetChild(i), expectedParseTree.Children.ElementAt(i));
        }

        #endregion

        #region General Test Methods

        /// <summary>
        /// Tests how the ANTLR4 parser handles empty command line parameters.
        /// </summary>
        [TestMethod]
        public void EmptyCommandLineParamentersTest()
        {
            // Lexes the input and checks whether there are no tokens
            CommandLineLexer lexer = this.LexInput(string.Empty);
            Assert.AreEqual(lexer.NextToken().Type, TokenConstants.Eof);

            // Parses the tokens and checks whether the resulting tree is empty
            CommandLineParser parser;
            IParseTree parseTree = this.ParseTokens(lexer, out parser);
            Assert.AreEqual(parseTree.ChildCount, 0);
        }

        #endregion

        #region Default Parameter Test Methods

        /// <summary>
        /// Tests how the ANTLR4 parser handles a single default parameter.
        /// </summary>
        [TestMethod]
        public void SingleDefaultParameterTest()
        {
            // Parses a single string default parameter
            CommandLineParser parser;
            IParseTree parseTree = this.ParseTokens(this.LexInput("abcXYZ"), out parser);

            // Validates the correctnes of the generated parse tree
            this.ValidateParseTree(parser, parseTree, new TreeNode
            {
                RuleName = "commandLine",
                Children = new List<TreeNode>
                {
                    new TreeNode
                    {
                        RuleName = "defaultParameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "abcXYZ"
                            }
                        }
                    }
                }
            });

            // Parses a single string default parameter
            parseTree = this.ParseTokens(this.LexInput("\"abc XYZ 123 ! § $ % & / ( ) = ? \\\""), out parser);

            // Validates the correctnes of the generated parse tree
            this.ValidateParseTree(parser, parseTree, new TreeNode
            {
                RuleName = "commandLine",
                Children = new List<TreeNode>
                {
                    new TreeNode
                    {
                        RuleName = "defaultParameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "\"abc XYZ 123 ! § $ % & / ( ) = ? \\\""
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Tests how the ANTLR4 parser handles multiple default parameters.
        /// </summary>
        [TestMethod]
        public void MutlipleDefaultParameterTest()
        {
            // Parses multiple default parameters
            CommandLineParser parser;
            IParseTree parseTree = this.ParseTokens(this.LexInput("abc \"123 456\" XYZ \"789 0\""), out parser);

            // Validates the correctnes of the generated parse tree
            this.ValidateParseTree(parser, parseTree, new TreeNode
            {
                RuleName = "commandLine",
                Children = new List<TreeNode>
                {
                    new TreeNode
                    {
                        RuleName = "defaultParameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "abc"
                            }
                        }
                    },
                    new TreeNode
                    {
                        RuleName = "defaultParameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "\"123 456\""
                            }
                        }
                    },
                    new TreeNode
                    {
                        RuleName = "defaultParameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "XYZ"
                            }
                        }
                    },
                    new TreeNode
                    {
                        RuleName = "defaultParameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "\"789 0\""
                            }
                        }
                    }
                }
            });
        }

        #endregion

        #region Parameter Test Methods

        /// <summary>
        /// Tests how the ANTLR4 parser handles Windows style switches.
        /// </summary>
        [TestMethod]
        public void WindowsStyleSwitchTest()
        {
            // Parses a Windows style switch
            CommandLineParser parser;
            IParseTree parseTree = this.ParseTokens(this.LexInput("/Switch"), out parser);

            // Validates the correctnes of the generated parse tree
            this.ValidateParseTree(parser, parseTree, new TreeNode
            {
                RuleName = "commandLine",
                Children = new List<TreeNode>
                {
                    new TreeNode
                    {
                        RuleName = "parameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "/Switch"
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Tests how the ANTLR4 parser handles Windows style parameters.
        /// </summary>
        [TestMethod]
        public void WindowsStyleParameterTest()
        {
            // Parses a Windows style parameter
            CommandLineParser parser;
            IParseTree parseTree = this.ParseTokens(this.LexInput("/Parameter:123"), out parser);

            // Validates the correctnes of the generated parse tree
            this.ValidateParseTree(parser, parseTree, new TreeNode
            {
                RuleName = "commandLine",
                Children = new List<TreeNode>
                {
                    new TreeNode
                    {
                        RuleName = "parameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "/Parameter"
                            },
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = ":"
                            },
                            new TreeNode
                            {
                                RuleName = "value",
                                Children = new List<TreeNode>
                                {
                                    new TreeNode
                                    {
                                        IsTerminalNode = true,
                                        Content = "123"
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Tests how the ANTLR4 parser handles UNIX style switches.
        /// </summary>
        [TestMethod]
        public void UnixStyleSwitchTest()
        {
            // Parses a UNIX style switch
            CommandLineParser parser;
            IParseTree parseTree = this.ParseTokens(this.LexInput("--Switch"), out parser);

            // Validates the correctnes of the generated parse tree
            this.ValidateParseTree(parser, parseTree, new TreeNode
            {
                RuleName = "commandLine",
                Children = new List<TreeNode>
                {
                    new TreeNode
                    {
                        RuleName = "parameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "--Switch"
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Tests how the ANTLR4 parser handles UNIX style parameters.
        /// </summary>
        [TestMethod]
        public void UnixStyleParameterTest()
        {
            // Parses a UNIX style parameter
            CommandLineParser parser;
            IParseTree parseTree = this.ParseTokens(this.LexInput("--Parameter=\"abc XYZ\""), out parser);

            // Validates the correctnes of the generated parse tree
            this.ValidateParseTree(parser, parseTree, new TreeNode
            {
                RuleName = "commandLine",
                Children = new List<TreeNode>
                {
                    new TreeNode
                    {
                        RuleName = "parameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "--Parameter"
                            },
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "="
                            },
                            new TreeNode
                            {
                                RuleName = "value",
                                Children = new List<TreeNode>
                                {
                                    new TreeNode
                                    {
                                        IsTerminalNode = true,
                                        Content = "\"abc XYZ\""
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Tests how the ANTLR4 parser handles UNIX style flagged switches.
        /// </summary>
        [TestMethod]
        public void UnixStyleFlaggedSwitchesTest()
        {
            // Parses UNIX style flagged switches
            CommandLineParser parser;
            IParseTree parseTree = this.ParseTokens(this.LexInput("-sUtZ"), out parser);

            // Validates the correctnes of the generated parse tree
            this.ValidateParseTree(parser, parseTree, new TreeNode
            {
                RuleName = "commandLine",
                Children = new List<TreeNode>
                {
                    new TreeNode
                    {
                        RuleName = "parameter",
                        Children = new List<TreeNode>
                        {
                            new TreeNode
                            {
                                IsTerminalNode = true,
                                Content = "-sUtZ"
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Tests how the ANTLR4 parser handles multiple parameters.
        /// </summary>
        [TestMethod]
        public void MultipleParameterTest()
        {

        }

        #endregion

        #region Mixed Default Parameter & Parameter Test Methods

        /// <summary>
        /// Tests how the ANTLR4 parser handles mixing of default parameters and parameters.
        /// </summary>
        [TestMethod]
        public void MixedDefaultParameterAndParameterTest()
        {

        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Represents a tree node, which can be used to validate the parse tree.
        /// </summary>
        public class TreeNode
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the name of the rule through which the tree node was parsed.
            /// </summary>
            public string RuleName { get; set; }

            /// <summary>
            /// Gets or sets the textual content of the tree node.
            /// </summary>
            public string Content { get; set; }

            /// <summary>
            /// Gets or sets the children of the tree node.
            /// </summary>
            public IEnumerable<TreeNode> Children { get; set; } = new List<TreeNode>();

            /// <summary>
            /// Gets or sets a value that determines whether this tree node is a leaf node.
            /// </summary>
            public bool IsTerminalNode { get; set; }

            #endregion
        }

        #endregion
    }
}
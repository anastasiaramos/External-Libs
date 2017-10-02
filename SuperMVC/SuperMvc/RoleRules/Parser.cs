/*	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Contributed by and used with permission from Nick Muhonen of 
 * Useable Concepts Inc. (http://www.useableconcepts.com/).
 * Copyright of this code is incorporated under the license terms
 * indicated in the AssemblyInfo.cs file of this project.
 *	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SuperMvc.RoleRules
{
	[DebuggerNonUserCode]
	public static class RoleEvaluator
	{
		public static bool EvaluateRole(IEnumerable<string>  roles, string rule)
		{
			return EvaluateRole(matcher => roles.Any(n => n.ToLowerInvariant() == rule.ToLowerInvariant()), rule);
		}

		public static bool EvaluateRole(Func<string,bool> matcher, string rule)
		{
			RoleParser parser = new RoleParser();
			IRule evaluator = parser.Parse(rule);
			return evaluator.Evaluate(matcher);
		}
	}

	[DebuggerNonUserCode]
	public class RoleParser
	{
		public IRule Parse(string rule)
		{
			State state = State.BeginRule;

			int currentPos = 0;
			char? current = null;

			Action nextChar = () => current =
				 rule == null || currentPos >= rule.Length
					  ? null as char?
					  : rule[currentPos++];

			Func<Exception> syntaxError = () =>
				 new ArgumentException(
							String.Format("A syntax error has occurred :{0}->{1}<-{2}",
								 rule.Substring(0, currentPos - 1),
								 rule[currentPos - 1],
								 currentPos == rule.Length ? "" : rule.Substring(currentPos)));

			StringBuilder wordBuffer = new StringBuilder();
			Stack<string> stack = new Stack<string>();

			nextChar();
			bool exit = false;
			while (!exit)
			{
				switch (state)
				{
					case State.BeginRule:
						{

							while (current.HasValue && Char.IsWhiteSpace(current.Value))
								nextChar();

							if (current.HasValue && current.Value == '!')
							{
								stack.Push(current.Value.ToString());
								nextChar();
								while (current.HasValue && Char.IsWhiteSpace(current.Value))
									nextChar();
							}


							if (current.HasValue)
							{
								if (current.Value == '(')
								{
									while (current.Value == '(')
									{
										stack.Push(current.Value.ToString());
										nextChar();
									}
									continue;
								}
								if (Char.IsLetterOrDigit(current.Value))
								{
									state = State.Word;
									continue;
								}
							}

							throw syntaxError();
						}
					case State.Word:
						{
							wordBuffer.Clear();
							int whiteSpacePos = 0;
							while (current.HasValue && Char.IsLetterOrDigit(current.Value))
							{
								while (current.HasValue && Char.IsLetterOrDigit(current.Value))
								{
									wordBuffer.Append(current.Value);
									nextChar();
								}
								whiteSpacePos = wordBuffer.Length;
								while (current.HasValue && Char.IsWhiteSpace(current.Value))
								{
									wordBuffer.Append(current.Value);
									nextChar();
								}
							}
							stack.Push(wordBuffer.ToString(0, whiteSpacePos));
							state = State.AfterRule;
							continue;
						}
					case State.AfterRule:
						{
							if (current.HasValue)
							{
								if (current.Value == ')')
								{
									while (current.HasValue && current.Value == ')')
									{
										stack.Push(current.Value.ToString());
										nextChar();
									}
									while (current.HasValue && Char.IsWhiteSpace(current.Value))
										nextChar();
								}
								else if (current.Value == '&' || current.Value == '|')
								{
									stack.Push(current.Value.ToString());
									nextChar();
									state = State.BeginRule;
								}
								else
									throw syntaxError();
							}
							else
							{
								exit = true;
							}
							continue;
						}
				}

			}

			return dumpStack(stack);
		}

		private IRule dumpStack(Stack<string> stack)
		{
			Stack<IRule> rules = new Stack<IRule>();

			while (stack.Count > 0)
			{
				string result = stack.Pop();

				switch (result)
				{
					case "!":
						{
							IRule lastRule = rules.Peek();
							IComparisonRule comparisonRule = lastRule as IComparisonRule;
							if (comparisonRule != null)
							{
								comparisonRule.LValue = new Not { InnerRule = comparisonRule.LValue };
							}
							else
							{
								rules.Pop();
								rules.Push(new Not { InnerRule = lastRule });
							}

							break;
						}
					case ")":
						{
							rules.Push(new GroupRule());
							break;
						}
					case "&":
						{
							IRule rValue = rules.Pop();
							rules.Push(new And { RValue = rValue });
							break;
						}
					case "|":
						{
							IRule rValue = rules.Pop();
							And andRule = rValue as And;
							if (rValue != null)
							{


							}

							rules.Push(new Or { RValue = rValue });
							break;
						}
					default:
						{
							IRule currentRule = null;
							if (result == "(")
							{
								if (rules.Count == 0)
									throw new ArgumentException("Missing an closing paren.");

								IRule innerRule = WeighAndOr(rules.Pop());
								if (rules.Count == 0)
									throw new ArgumentException("Missing an closing paren.");

								GroupRule groupRule = rules.Pop() as GroupRule;
								groupRule.InnerRule = innerRule;
								currentRule = groupRule;
							}
							else
								currentRule = new Match { Role = result };

							if (rules.Count > 0)
							{
								IComparisonRule rule = rules.Peek() as IComparisonRule;
								if (rule != null)
								{
									rules.Pop();
									rule.LValue = currentRule;
									currentRule = rule;
								}
							}
							rules.Push(currentRule);
							break;
						}
				}

			}


			if (rules.Count > 1)
				throw new ArgumentException("Missing an opening paren.");

			IRule finalRule = WeighAndOr(rules.Pop());

			return finalRule;

		}

		private IRule WeighAndOr(IRule parentRule)
		{
			IRule currentRule = parentRule;
			Or previousRule = null;

			while (true)
			{
				IComparisonRule comparisonRule = currentRule as IComparisonRule;
				if (comparisonRule == null)
					break;

				if (comparisonRule is Or)
				{
					previousRule = currentRule as Or;
					currentRule = comparisonRule.RValue;
					continue;
				}

				And andRule = comparisonRule as And;
				while (andRule.RValue is And)
					andRule = andRule.RValue as And;

				Or bottomOr = andRule.RValue as Or;
				if (bottomOr != null)
				{
					IRule orLeft = bottomOr.LValue;
					bottomOr.LValue = andRule;
					andRule.RValue = orLeft;
					if (previousRule == null)
						parentRule = previousRule = bottomOr;
					else
					{
						previousRule.RValue = bottomOr;
						previousRule = bottomOr;
					}
				}
				else
					break;
				currentRule = bottomOr.RValue;
			}
			return parentRule;
		}


		class RulePair
		{
			public IRule Start, Current;

		}


		enum State
		{
			BeginRule,
			Word,
			AfterRule,
		}


	}





	//public class SimpleScanner
	//{
	//    string value;
	//    int currentPos = 0;

	//    public SimpleScanner(string value)
	//    {
	//        this.value = value;

	//    }

	//    public char? NextChar()
	//    {
	//        if (value == null || currentPos >= value.Length)
	//            return null;

	//        return value[currentPos++];
	//    }

	//}
}

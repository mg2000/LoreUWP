using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Lore
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class NewGamePage : Page
	{
		private QuestionInfo[] mQuestionList = {
			new QuestionInfo("당신이 한 밤중에 공부하고 있을때 밖에서 무슨 소리가 들렸다",
				new string[] {
					"1] 밖으로 나가서 알아본다",
					"2] 그 소리가 무엇일까 생각을 한다",
					"3] 공부에만 열중한다"
				},
				new int[] { 0, 1, 2 }),
			new QuestionInfo("당신은 체력장 오래달리기에서 포기할 수 없는 한 바퀴를 남겨 놓고\r\n거의 탈진 상태가 되었다",
				new string[] {
					"1] 힘으로 밀고 나간다",
					"2] 정신력으로 버티며 달린다",
					"3] 그래도 여태까지와 마찬가지로 달린다"
				},
				new int[] { 0, 1, 3 }),
			new QuestionInfo("당신은 이 게임 속에서 적들에게 완전히 포위되어 승산 없이 싸우고\r\n있다",
				new string[] {
					"1] 힘이 남아 있는한 죽을때까지 싸운다",
					"2] 한가지라도 탈출할 가능성을 찾는다",
					"3] 일단 싸우면서 여러 방법을 생각한다"
				},
				new int[] { 0, 1, 4 }),
			new QuestionInfo("당신은 매우 복잡한 매듭을 풀어야하는 일이 생겼다",
				new string[] {
					"1] 칼로 매듭을 잘라 버린다",
					"2] 매듭의 끝부분 부터 차근차근 훓어본다",
					"3] 어쨌든 계속 풀려고 손을 놀린다"
				},
				new int[] { 0, 2, 3 }),
			new QuestionInfo("허허 벌판을 걸어가던 당신은 갑작스런 우박을 만난다",
				new string[] {
					"1] 당항한 나머지 피할곳을 찾아 뛴다",
					"2] 침착하게 주위를 살펴 안전한곳을 찾는다",
					"3] 우박 정도는 그냥 견딘다"
				},
				new int[] { 0, 2, 4 }),
			new QuestionInfo("집안에 불이나서 탈출하려는데 나무로 만든 문이 좀처럼 열리지\r\n않는다",
				new string[] {
					"1] 다른 탈출구를 찾아간다",
					"2] 1] 번과 같은 불확실한 도전을 하는것 보다는 확실한\n 탈출구인 이 문을 끝까지 열려한다",
					"3] 나무문이 타서 구멍이 생길때까지 기다려 탈출한다"
				},
				new int[] { 0, 3, 4 }),
			new QuestionInfo("고대에 태어난 당신은, 한날 당신의 눈앞에서 물체가 사라지는\r\n마술을 보았을때 당신의 해석은 ?",
				new string[] {
					"1] 이것은 마법이다",
					"2] 이것은 사람의 새로운 능력이다",
					"3] 단순한 사람의 눈속임이다"
				},
				new int[] { 1, 2, 3 }),
			new QuestionInfo("시험 기간에 당신이 도서관에서 공부를 하려는데 주위가 너무\r\n시끄럽다",
				new string[] {
					"1] 상관없이 참으며 공부한다",
					"2] 너무 공부를 열심히해서 그런 소리가 안 들린다",
					"3] 시끄러움에 대항하는 마음으로 공부한다"
				},
				new int[] { 1, 2, 4 }),
			new QuestionInfo("직장 생활을 하던 당신은 아무 이유없이 상관에게 심한 욕을 들었다",
				new string[] {
					"1] 겉으로는 순종하면서 속으로는 감정을 샇는다",
					"2] 웬만하면 참고 넘긴다",
					"3] 상관인걸 무시하고 이유를 들라며 대든다"
				},
				new int[] { 1, 3, 4 }),
			new QuestionInfo("당신이 새로운 프로그램을 짜던중 알수없는 오류가 생겼다",
				new string[] {
					"1] 차근차근 순서도를 생각하며 오류를 찾는다",
					"2] 여러번 실행 시키며 오류를 찾는다",
					"3] 오류가 작으면 그냥 사용한다"
				},
				new int[] { 2, 3, 4 }),

		};

		private enum FocusItem
		{
			Name,
			Gender,
			Question,
			AssignPoint,
		}
		private FocusItem mFocusItem = FocusItem.Name;
		private string mFocusGender = "male";
		private int mQuestionID = 0;
		private int mAnswerID = 0;
		private readonly int[] mTransdata = { 0, 0, 0, 0, 0 };
		private int mRemainPoint = 40;
		private int mAssignID = 0;
		
		private readonly List<Lore> mPlayerList = new List<Lore>();

		public NewGamePage()
		{
			this.InitializeComponent();

			mPlayerList.Add(new Lore());

			TypedEventHandler<CoreWindow, KeyEventArgs> newGamePageKeyEvent = null;
			newGamePageKeyEvent = (sender, args) =>
			{
				void ShowQuestion() {
					mAnswerID = 0;

					AnswerLabel1.Foreground = new SolidColorBrush(Colors.Yellow);
					AnswerLabel2.Foreground = new SolidColorBrush(Colors.LightCyan);
					AnswerLabel3.Foreground = new SolidColorBrush(Colors.LightCyan);

					QuestionLabel.Text = mQuestionList[mQuestionID].Question;
					AnswerLabel1.Text = mQuestionList[mQuestionID].Answer[0];
					AnswerLabel2.Text = mQuestionList[mQuestionID].Answer[1];
					AnswerLabel3.Text = mQuestionList[mQuestionID].Answer[2];
				}

				Debug.WriteLine($"키보드 테스트: {args.VirtualKey}");

				if (mFocusItem == FocusItem.Name)
				{
					if (args.VirtualKey == VirtualKey.Enter)
					{
						if (UserNameText.Text == "")
							new MessageDialog("이름을 입력해 주십시오.", "이름 미입력").ShowAsync();
						else
						{
							mPlayerList[0].Name = UserNameText.Text;
							UserNameResultLabel.Text = $"당신의 이름은 {mPlayerList[0].Name}입니다.";

							UserNameInputPanel.Visibility = Visibility.Collapsed;
							UserNameResultPanel.Visibility = Visibility.Visible;
							UserGenderInputPanel.Visibility = Visibility.Visible;

							mFocusItem = FocusItem.Gender;
						}
					}
				}
				else if (mFocusItem == FocusItem.Gender)
				{
					if (args.VirtualKey == VirtualKey.Left)
					{
						UserGenderMale.Foreground = new SolidColorBrush(Colors.LightGreen);
						UserGenderFemale.Foreground = new SolidColorBrush(Colors.Black);

						mFocusGender = "male";
					}
					else if (args.VirtualKey == VirtualKey.Right)
					{
						UserGenderMale.Foreground = new SolidColorBrush(Colors.Black);
						UserGenderFemale.Foreground = new SolidColorBrush(Colors.LightGreen);

						mFocusGender = "female";
					}
					else if (args.VirtualKey == VirtualKey.Enter)
					{
						mPlayerList[0].Gender = mFocusGender;
						var genderKor = mPlayerList[0].Gender == "male" ? "남성" : "여성";
						UserGenderResultLabel.Text = $"당신의 성별은 {genderKor}입니다.";

						UserGenderInputPanel.Visibility = Visibility.Collapsed;
						UserGenderResultPanel.Visibility = Visibility.Visible;

						ShowQuestion();

						QuestionTitle.Visibility = Visibility.Visible;
						QuestionLabel.Visibility = Visibility.Visible;
						AnswerPanel.Visibility = Visibility.Visible;

						mFocusItem = FocusItem.Question;
					}
				}
				else if (mFocusItem == FocusItem.Question) {
					if (args.VirtualKey == VirtualKey.Up)
					{
						if (mAnswerID == 0) {
							AnswerLabel1.Foreground = new SolidColorBrush(Colors.LightCyan);
							AnswerLabel2.Foreground = new SolidColorBrush(Colors.LightCyan);
							AnswerLabel3.Foreground = new SolidColorBrush(Colors.Yellow);

							mAnswerID = 2;
						}
						else if (mAnswerID == 1) {
							AnswerLabel1.Foreground = new SolidColorBrush(Colors.Yellow);
							AnswerLabel2.Foreground = new SolidColorBrush(Colors.LightCyan);
							AnswerLabel3.Foreground = new SolidColorBrush(Colors.LightCyan);

							mAnswerID = 0;

						}
						else if (mAnswerID == 2) {
							AnswerLabel1.Foreground = new SolidColorBrush(Colors.LightCyan);
							AnswerLabel2.Foreground = new SolidColorBrush(Colors.Yellow);
							AnswerLabel3.Foreground = new SolidColorBrush(Colors.LightCyan);

							mAnswerID = 1;
						}
						
					}
					else if (args.VirtualKey == VirtualKey.Down)
					{
						if (mAnswerID == 0)
						{
							AnswerLabel1.Foreground = new SolidColorBrush(Colors.LightCyan);
							AnswerLabel2.Foreground = new SolidColorBrush(Colors.Yellow);
							AnswerLabel3.Foreground = new SolidColorBrush(Colors.LightCyan);

							mAnswerID = 1;
							
						}
						else if (mAnswerID == 1)
						{
							AnswerLabel1.Foreground = new SolidColorBrush(Colors.LightCyan);
							AnswerLabel2.Foreground = new SolidColorBrush(Colors.LightCyan);
							AnswerLabel3.Foreground = new SolidColorBrush(Colors.Yellow);

							mAnswerID = 2;
						}
						else if (mAnswerID == 2)
						{
							AnswerLabel1.Foreground = new SolidColorBrush(Colors.Yellow);
							AnswerLabel2.Foreground = new SolidColorBrush(Colors.LightCyan);
							AnswerLabel3.Foreground = new SolidColorBrush(Colors.LightCyan);

							mAnswerID = 0;
						}
					}
					else if (args.VirtualKey == VirtualKey.Enter) {
						mQuestionID++;
						if (mQuestionID < mQuestionList.Length)
						{
							mTransdata[mAnswerID]++;
							ShowQuestion();
						}
						else {
							for (var i = 0; i < mTransdata.Length; i++) {
								switch (mTransdata[i]) {
									case 0:
										mTransdata[i] = 5;
										break;
									case 1:
										mTransdata[i] = 7;
										break;
									case 2:
										mTransdata[i] = 11;
										break;
									case 3:
										mTransdata[i] = 14;
										break;
									case 4:
										mTransdata[i] = 17;
										break;
									case 5:
										mTransdata[i] = 19;
										break;
									case 6:
										mTransdata[i] = 20;
										break;
									default:
										mTransdata[i] = 10;
										break;
								}
							}

							mPlayerList[0].Strength = mTransdata[0];
							mPlayerList[0].Mentality = mTransdata[1];
							mPlayerList[0].Concentration = mTransdata[2];
							mPlayerList[0].Endurance = mTransdata[3];
							mPlayerList[0].Resistance = mTransdata[4];

							var offset = 4;
							if (mPlayerList[0].Gender == "male") {
								mPlayerList[0].Strength += offset;
								if (mPlayerList[0].Strength <= 20)
									offset = 0;
								else {
									offset = mPlayerList[0].Strength - 20;
									mPlayerList[0].Strength = 20;
								}

								mPlayerList[0].Endurance += offset;
								if (mPlayerList[0].Endurance <= 20)
									offset = 0;
								else
								{
									offset = mPlayerList[0].Endurance - 20;
									mPlayerList[0].Endurance = 20;
								}

								mPlayerList[0].Resistance += offset;
								if (mPlayerList[0].Resistance > 20)
									mPlayerList[0].Resistance = 20;
							}
							else {
								mPlayerList[0].Mentality += offset;
								if (mPlayerList[0].Mentality <= 20)
									offset = 0;
								else
								{
									offset = mPlayerList[0].Mentality - 20;
									mPlayerList[0].Mentality = 20;
								}

								mPlayerList[0].Concentration += offset;
								if (mPlayerList[0].Concentration <= 20)
									offset = 0;
								else
								{
									offset = mPlayerList[0].Concentration - 20;
									mPlayerList[0].Concentration = 20;
								}

								mPlayerList[0].Resistance += offset;
								if (mPlayerList[0].Resistance > 20)
									mPlayerList[0].Resistance = 20;
							}

							DefaultInputGrid.Visibility = Visibility.Collapsed;

							mPlayerList[0].Gender = mFocusGender;
							var genderKor = mPlayerList[0].Gender == "male" ? "남성" : "여성";

							UserNameResultLabel.Text = $"당신의 이름은 {mPlayerList[0].Name}입니다.";
							UserNameFinalText.Text = $"당신의 성별은 {genderKor}입니다.";

							StrengthText.Text = mPlayerList[0].Strength.ToString();
							MentalityText.Text = mPlayerList[0].Mentality.ToString();
							ConcentrationText.Text = mPlayerList[0].Concentration.ToString();
							EnduranceText.Text = mPlayerList[0].Endurance.ToString();
							ResistanceText.Text = mPlayerList[0].Resistance.ToString();

							mPlayerList[0].HP = mPlayerList[0].Endurance;
							mPlayerList[0].SP = mPlayerList[0].Mentality;
							mPlayerList[0].ESP = mPlayerList[0].Concentration;

							HPText.Text = mPlayerList[0].HP.ToString();
							SPText.Text = mPlayerList[0].SP.ToString();
							ESPText.Text = mPlayerList[0].ESP.ToString();

							RemainPointText.Text = mRemainPoint.ToString();

							for (var i = 0; i < 3; i++)
								mTransdata[i] = 0;

							ExtraInputGrid.Visibility = Visibility.Visible;

							mAssignID = 0;
							mFocusItem = FocusItem.AssignPoint;
						}
					}
				}
				else if (mFocusItem == FocusItem.AssignPoint) {
					void UpdateStat() {
						RemainPointText.Text = mRemainPoint.ToString();

						if (mAssignID == 0)
							AgilityText.Text = mTransdata[0].ToString();

						if (mAssignID == 1)
							AccuracyText.Text = mTransdata[1].ToString();

						if (mAssignID == 2)
							LuckText.Text = mTransdata[2].ToString();
					}

					if (args.VirtualKey == VirtualKey.Left)
					{
						if (mTransdata[mAssignID] > 0) {
							mTransdata[mAssignID]--;
							mRemainPoint++;

							UpdateStat();
						}

					}
					else if (args.VirtualKey == VirtualKey.Right)
					{
						if (mTransdata[mAssignID] < 20 && mRemainPoint > 0)
						{
							mTransdata[mAssignID]++;
							mRemainPoint--;

							UpdateStat();
						}
					}
				}
			};

			Window.Current.CoreWindow.KeyUp += newGamePageKeyEvent;
		}

		private class QuestionInfo {
			public string Question {
				get;
				private set;
			}

			public string[] Answer {
				get;
				private set;
			}

			public int[] PointID {
				get;
				private set;
			}

			public QuestionInfo(string question, string[] answer, int[] pointID) {
				Question = question;
				Answer = answer;
				PointID = pointID;
			}
		}
	}
}

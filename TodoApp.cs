using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

public class TodoItem {
 public string Title { get; set; }
 public bool Done { get; set; }
 public override string ToString() { return Title; }
}
public class TodoWindow : Form {
 readonly TextBox input = new TextBox();
 readonly CheckedListBox list = new CheckedListBox();
 readonly Label summary = new Label();
 readonly Label status = new Label();
 readonly List<TodoItem> items = new List<TodoItem>();
 readonly string dataPath;
 bool loading = false;
 bool writable = true;
 public TodoWindow(string path, bool readSaved) {
  dataPath = path;
  Text = "나의 할 일 · 제어 테스트"; ClientSize = new Size(620, 480);
  MinimumSize = new Size(460, 360); StartPosition = FormStartPosition.CenterScreen;
  Font = new Font("Malgun Gothic", 10); BackColor = Color.FromArgb(244,247,250);
  var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 1, RowCount = 6 };
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
  layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
  var heading = new Label { Text = "나의 할 일", Font = new Font(Font.FontFamily, 22, FontStyle.Bold), Dock = DockStyle.Fill };
  var entry = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = new Padding(0) };
  entry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); entry.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
  input.Dock = DockStyle.Fill; input.MaxLength = 120; input.AccessibleName = "새 할 일";
  var add = new Button { Text = "추가", Dock = DockStyle.Fill };
  add.Click += delegate { AddTask(); }; entry.Controls.Add(input,0,0); entry.Controls.Add(add,1,0);
  summary.Dock = DockStyle.Fill;
  list.Dock = DockStyle.Fill; list.CheckOnClick = true; list.IntegralHeight = false; list.HorizontalScrollbar = true;
  list.ItemCheck += delegate(object sender, ItemCheckEventArgs e) {
   if (loading) return;
   items[e.Index].Done = e.NewValue == CheckState.Checked;
   Save(); UpdateSummary();
  };
  var delete = new Button { Text = "선택한 할 일 삭제", Dock = DockStyle.Fill };
  delete.Click += delegate { DeleteTask(); };
  status.Dock = DockStyle.Fill; status.ForeColor = Color.FromArgb(90,100,115);
  status.Text = "이 폴더의 할일데이터.json 파일에 자동 저장됩니다.";
  layout.Controls.Add(heading,0,0); layout.Controls.Add(entry,0,1); layout.Controls.Add(summary,0,2);
  layout.Controls.Add(list,0,3); layout.Controls.Add(delete,0,4); layout.Controls.Add(status,0,5);
  Controls.Add(layout); AcceptButton = add;
  if (readSaved) LoadData(); RefreshList();
 }
 void LoadData() {
  try {
   if (!File.Exists(dataPath)) return;
   var loaded = new JavaScriptSerializer().Deserialize<List<TodoItem>>(File.ReadAllText(dataPath,Encoding.UTF8));
   if (loaded == null || loaded.Exists(t => t == null || String.IsNullOrWhiteSpace(t.Title) || t.Title.Length > 120)) throw new Exception("Invalid data");
   items.AddRange(loaded);
  } catch { writable = false; status.Text = "기존 기록 읽기 실패: 원본 보호를 위해 이번 변경은 저장하지 않습니다."; }
 }
 void Save() {
  if (!writable) return;
  try {
   string temp = dataPath + ".tmp";
   File.WriteAllText(temp,new JavaScriptSerializer().Serialize(items),Encoding.UTF8);
   if (File.Exists(dataPath)) File.Replace(temp,dataPath,null); else File.Move(temp,dataPath);
   status.Text = "저장됨 · " + DateTime.Now.ToString("HH:mm:ss");
  } catch { status.Text = "저장 실패: 폴더의 쓰기 권한과 디스크 공간을 확인하세요."; }
 }
 void UpdateSummary() { summary.Text = String.Format("전체 {0}개 · 완료 {1}개",items.Count,items.FindAll(t => t.Done).Count); }
 void RefreshList() {
  loading = true; list.Items.Clear();
  foreach (var t in items) list.Items.Add(t,t.Done);
  loading = false; UpdateSummary();
 }
 void AddTask() {
  string title = input.Text.Trim();
  if (title.Length == 0) { status.Text = "할 일을 입력해 주세요. 공백만 입력할 수 없습니다."; input.Focus(); return; }
  items.Add(new TodoItem {Title=title,Done=false}); Save(); RefreshList(); input.Clear(); input.Focus();
 }
 void DeleteTask() {
  int index = list.SelectedIndex;
  if (index < 0) { status.Text = "삭제할 항목을 먼저 선택하세요."; return; }
  items.RemoveAt(index); Save(); RefreshList();
 }
 public static void SmokeTest(string path) {
  using (var window = new TodoWindow(path,false)) {
   window.CreateControl(); window.input.Text = "자동 검사 할 일"; window.AddTask();
   if (window.list.Items.Count != 1) throw new Exception("Add failed");
   window.list.SetItemChecked(0,true);
   if (!window.items[0].Done) throw new Exception("Check failed");
  }
  using (var restored = new TodoWindow(path,true)) {
   if (restored.items.Count != 1 || !restored.items[0].Done) throw new Exception("Restore failed");
   restored.list.SelectedIndex = 0; restored.DeleteTask();
   if (restored.items.Count != 0) throw new Exception("Delete failed");
   restored.input.Text = "   "; restored.AddTask();
   if (restored.items.Count != 0) throw new Exception("Validation failed");
  }
 }
 [STAThread]
 public static int Main(string[] args) {
  Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
  string folder = AppDomain.CurrentDomain.BaseDirectory;
  if (args.Length > 0 && args[0] == "--self-test") {
   string temp = Path.Combine(folder,"native-test-data.json");
   try {
    SmokeTest(temp);
    File.WriteAllText(Path.Combine(folder,"실행파일-검사결과.txt"),"PASS: native form creation, add, check, JSON save and reload, delete, blank validation.\r\n" + DateTime.Now.ToString("s"),Encoding.UTF8);
    File.Delete(temp); return 0;
   } catch (Exception ex) {
    File.WriteAllText(Path.Combine(folder,"실행파일-검사결과.txt"),"FAIL: " + ex,Encoding.UTF8); return 1;
   }
  }
  Application.Run(new TodoWindow(Path.Combine(folder,"할일데이터.json"),true)); return 0;
 }
}

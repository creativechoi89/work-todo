using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WorkTodo {
 public static class Theme {
  public static readonly Color Ink = Color.FromArgb(28,43,64);
  public static readonly Color Green = Color.FromArgb(24,104,85);
  public static Button Button(string text, bool primary) {
   return new Button { Text=text, AutoSize=true, Height=34, Padding=new Padding(12,4,12,4), Margin=new Padding(0,0,8,0), FlatStyle=FlatStyle.Flat, BackColor=primary?Green:Color.White, ForeColor=primary?Color.White:Ink, UseVisualStyleBackColor=false };
  }
 }
 public class EditDialog : Form {
  TextBox title = new TextBox(); TextBox body = new TextBox(); TextBox tag = new TextBox();
  ComboBox category = new ComboBox(); DateTimePicker due = new DateTimePicker();
  public string ItemTitle { get {return Model.CleanTitle(title.Text);} }
  public string ItemBody { get {return Model.CleanBody(body.Text);} }
  public string ItemCategory { get {return category.SelectedItem.ToString();} }
  public string ItemDue { get {return due.Checked?due.Value.ToString("yyyy-MM-dd"):"";} }
  public string ItemTag {get {return tag.Text.Trim();}}
  public EditDialog(bool note, string caption, string initialTitle, string initialBody, string group, string date, string initialTag) {
   Text = caption; Font = new Font("Malgun Gothic",10); ClientSize=new Size(620,540); MinimumSize=new Size(480,440);
   StartPosition=FormStartPosition.CenterParent; ShowInTaskbar=false; BackColor=Color.White;
   var layout = new TableLayoutPanel { Dock=DockStyle.Fill, Padding=new Padding(24), ColumnCount=1, RowCount=7 };
   foreach(float height in new float[]{24,38,24,40,24}) layout.RowStyles.Add(new RowStyle(SizeType.Absolute,height));
   layout.RowStyles.Add(new RowStyle(SizeType.Percent,100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,48));
   title.Dock=DockStyle.Fill; title.MaxLength=200; title.Text=initialTitle; title.AccessibleName="제목";
   body.Dock=DockStyle.Fill; body.Multiline=true; body.ScrollBars=ScrollBars.Vertical; body.MaxLength=40000; body.Text=initialBody; body.AcceptsReturn=true; body.AccessibleName="상세내용";
   category.DropDownStyle=ComboBoxStyle.DropDownList; category.Width=150; category.Items.AddRange(note?new string[]{"설비 참고사항","업무 참고사항"}:Model.Categories); category.SelectedItem=group;
   if(category.SelectedIndex<0)category.SelectedIndex=0;
   category.AccessibleName="분류";
   var row=new FlowLayoutPanel{Dock=DockStyle.Fill,Margin=new Padding(0)}; row.Controls.Add(category);
   if(note) {
    row.Controls.Add(new Label{Text="설비명 / 구분",AutoSize=true,Margin=new Padding(14,5,4,0)});
    tag.Width=220;tag.MaxLength=200;tag.Text=initialTag;tag.AccessibleName="설비명 또는 구분";row.Controls.Add(tag);
   } else {
    row.Controls.Add(new Label{Text="예정일 (선택)",AutoSize=true,Margin=new Padding(14,5,4,0)});
    due.Format=DateTimePickerFormat.Custom;due.CustomFormat="yyyy-MM-dd";due.ShowCheckBox=true;due.Width=150;due.Checked=false;due.AccessibleName="예정일";
    DateTime parsed;if(DateTime.TryParse(date,out parsed)){due.Value=parsed;due.Checked=true;}row.Controls.Add(due);
   }
   var buttons=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(0,10,0,0)};
   var save=Theme.Button("저장",true);var cancel=Theme.Button("취소",false);cancel.DialogResult=DialogResult.Cancel;
   save.Click+=delegate {try {var t=ItemTitle;var b=ItemBody;DialogResult=DialogResult.OK;Close();}catch(Exception ex){MessageBox.Show(this,ex.Message,"입력 확인",MessageBoxButtons.OK,MessageBoxIcon.Information);}};
   buttons.Controls.Add(save);buttons.Controls.Add(cancel);
   layout.Controls.Add(new Label{Text="제목 *",Dock=DockStyle.Fill},0,0);layout.Controls.Add(title,0,1);
   layout.Controls.Add(new Label{Text=note?"참고사항 종류 및 구분":"분류 및 예정일",Dock=DockStyle.Fill},0,2);layout.Controls.Add(row,0,3);
   layout.Controls.Add(new Label{Text="상세내용",Dock=DockStyle.Fill},0,4);layout.Controls.Add(body,0,5);layout.Controls.Add(buttons,0,6);
   Controls.Add(layout);CancelButton=cancel;
   Shown+=delegate{title.Focus();};
  }
 }
 public class MainWindow : Form {
  readonly DataStore store;
  readonly TabControl tabs=new TabControl();
  readonly ListView list=new ListView();
  readonly TextBox preview=new TextBox();
  readonly TextBox search=new TextBox();
  readonly Label count=new Label();
  readonly ToolStripStatusLabel status=new ToolStripStatusLabel();
  Button add,edit,done,delete;
  bool refreshing; bool uiReady;
  public MainWindow(DataStore dataStore, bool demo) {
   store=dataStore;Text="할일관리 v0.2.0"+(demo?" · 미리보기 (예시 데이터)":"");
   Font=new Font("Malgun Gothic",10);ForeColor=Theme.Ink;BackColor=Color.FromArgb(243,246,249);
   ClientSize=new Size(1100,740);MinimumSize=new Size(860,600);StartPosition=FormStartPosition.CenterScreen;
   var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=7,Padding=new Padding(24,16,24,12)};
   foreach(float h in new float[]{72,44,44,38,30})layout.RowStyles.Add(new RowStyle(SizeType.Absolute,h));
   layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,28));
   var header=new Panel{Dock=DockStyle.Fill};
   header.Controls.Add(new Label{Text="업무를 한눈에, 기록은 차곡차곡",Font=new Font(Font.FontFamily,20,FontStyle.Bold),AutoSize=true,Location=new Point(0,0)});
   header.Controls.Add(new Label{Text=DateTime.Now.ToString("yyyy년 M월 d일 dddd")+"  ·  이 PC에 저장  ·  인터넷 연결 불필요",AutoSize=true,ForeColor=Color.FromArgb(95,113,132),Location=new Point(2,42)});
   var menu=new FlowLayoutPanel{Dock=DockStyle.Fill,Margin=new Padding(0)};
   var backup=Theme.Button("백업 저장",false);backup.Click+=delegate{ExportBackup();};
   var restore=Theme.Button("백업 복원",false);restore.Click+=delegate{RestoreBackup();};
   var legacy=Theme.Button("기존 목록 가져오기",false);legacy.Click+=delegate{ImportLegacy();};
   var folder=Theme.Button("저장 위치 안내",false);folder.Click+=delegate{MessageBox.Show(this,"현재 데이터:\r\n"+store.DataPath+"\r\n\r\n저장 전 기록은 같은 폴더의 backups에 자동 보관됩니다.\r\n업데이트 전 ‘백업 저장’으로 다른 위치에도 보관해 주세요.","저장 위치",MessageBoxButtons.OK,MessageBoxIcon.Information);};
   menu.Controls.AddRange(new Control[]{backup,restore,legacy,folder});
   tabs.Dock=DockStyle.Fill;tabs.Font=new Font(Font.FontFamily,11);tabs.Padding=new Point(16,6);
   foreach(string name in new []{"금일사항","진행사항","예정사항","완료","설비 참고사항","업무 참고사항"})tabs.TabPages.Add(name);
   tabs.SelectedIndexChanged+=delegate{if(uiReady)RefreshItems(null);};
   var toolbar=new FlowLayoutPanel{Dock=DockStyle.Fill,Margin=new Padding(0)};
   add=Theme.Button("+ 새 업무",true);add.Click+=delegate{EditItem(true);};
   edit=Theme.Button("수정",false);edit.Click+=delegate{EditItem(false);};
   done=Theme.Button("선택 완료",false);done.Click+=delegate{CompleteSelected();};
   delete=Theme.Button("삭제",false);delete.Click+=delegate{DeleteSelected();};
   toolbar.Controls.AddRange(new Control[]{add,edit,done,delete,new Label{Text="검색",AutoSize=true,Margin=new Padding(6,7,6,0)}});
   search.Width=210;search.AccessibleName="현재 목록 검색";search.TextChanged+=delegate{RefreshItems(null);};toolbar.Controls.Add(search);
   count.Dock=DockStyle.Fill;count.ForeColor=Color.FromArgb(80,99,119);
   var split=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,SplitterWidth=7};
   // Establish a real initial size before assigning panel constraints.
   split.Size=new Size(1020,410);split.Panel1MinSize=150;split.Panel2MinSize=90;split.SplitterDistance=260;
   list.Dock=DockStyle.Fill;list.View=View.Details;list.FullRowSelect=true;list.HideSelection=false;list.MultiSelect=true;list.BorderStyle=BorderStyle.FixedSingle;
   list.Columns.Add("제목",420);list.Columns.Add("분류 / 구분",140);list.Columns.Add("예정일",110);list.Columns.Add("수정일",160);list.Columns.Add("완료일",160);
   list.AccessibleName="업무 및 참고사항 목록";list.SelectedIndexChanged+=delegate{if(!refreshing)ShowPreview();};
   list.DoubleClick+=delegate{EditItem(false);};
   preview.Dock=DockStyle.Fill;preview.Multiline=true;preview.ReadOnly=true;preview.ScrollBars=ScrollBars.Vertical;preview.BackColor=Color.White;preview.BorderStyle=BorderStyle.FixedSingle;preview.AccessibleName="선택 항목 상세내용";
   split.Panel1.Controls.Add(list);split.Panel2.Controls.Add(preview);
   var hint=new Label{Text="Ctrl / Shift로 여러 업무 선택 · 더블 클릭으로 수정 · 날짜가 지나도 미완료 업무는 유지됩니다.",Dock=DockStyle.Fill,ForeColor=Color.FromArgb(95,113,132),Font=new Font(Font.FontFamily,9)};
   layout.Controls.Add(header,0,0);layout.Controls.Add(menu,0,1);layout.Controls.Add(tabs,0,2);layout.Controls.Add(toolbar,0,3);layout.Controls.Add(count,0,4);layout.Controls.Add(split,0,5);layout.Controls.Add(hint,0,6);
   var statusStrip=new StatusStrip();status.Spring=true;status.TextAlign=ContentAlignment.MiddleLeft;statusStrip.Items.Add(status);
   Controls.Add(layout);Controls.Add(statusStrip);
   status.Text=store.LoadError??(store.ImportedLegacy?"기존 목록을 금일사항으로 가져왔습니다. 원본은 보존했습니다.":"준비됨 · 변경 내용은 저장 성공 후 반영됩니다.");
   KeyPreview=true;KeyDown+=delegate(object sender,KeyEventArgs e){if(e.Control&&e.KeyCode==Keys.N){EditItem(true);e.Handled=true;}};
   tabs.SelectedIndex=0; uiReady=true; RefreshItems(null);
   Shown+=delegate{if(store.LoadError!=null)MessageBox.Show(this,store.LoadError,"데이터 보호 모드",MessageBoxButtons.OK,MessageBoxIcon.Warning);};
  }
  bool IsNotes {get{return tabs.SelectedIndex>=4;}}
  string NoteKind {get{return tabs.SelectedIndex==4?"equipment":"work";}}
  string CurrentCategory {get{return Model.Categories[Math.Min(tabs.SelectedIndex,2)];}}
  static string DisplayDate(string timestamp) {DateTimeOffset d;return DateTimeOffset.TryParse(timestamp,out d)?d.LocalDateTime.ToString("yyyy-MM-dd HH:mm"):"";}
  bool Matches(params string[] fields){string query=search.Text.Trim();return query.Length==0||fields.Any(s=>(s??"").IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0);}
  List<string> SelectedIds(){return list.SelectedItems.Cast<ListViewItem>().Select(i=>(string)i.Tag).ToList();}
  void RefreshItems(string selectId) {
   refreshing=true;list.BeginUpdate();list.Items.Clear();
   if(store.LoadError==null) {
    if(IsNotes) {
     foreach(var n in store.Data.Notes.Where(n=>n.Kind==NoteKind&&Matches(n.Title,n.Body,n.Tag)).OrderByDescending(n=>n.UpdatedAt)) {
      var row=new ListViewItem(new[]{n.Title,n.Tag,"",DisplayDate(n.UpdatedAt),""});row.Tag=n.Id;list.Items.Add(row);if(n.Id==selectId)row.Selected=true;
     }
    } else {
     bool completed=tabs.SelectedIndex==3;
     foreach(var t in store.Data.Tasks.Where(t=>t.Done==completed&&(completed||t.Category==CurrentCategory)&&Matches(t.Title,t.Body,t.Category)).OrderByDescending(t=>completed?t.CompletedAt:t.UpdatedAt)) {
      var row=new ListViewItem(new[]{t.Title,t.Category,t.DueDate,DisplayDate(t.UpdatedAt),DisplayDate(t.CompletedAt)});row.Tag=t.Id;list.Items.Add(row);if(t.Id==selectId)row.Selected=true;
     }
    }
   }
   list.EndUpdate();refreshing=false;
   add.Text=IsNotes?"+ 새 참고사항":"+ 새 업무";
   done.Visible=!IsNotes;done.Text=tabs.SelectedIndex==3?"완료 취소":"선택 완료";
   bool healthy=store.LoadError==null;add.Enabled=healthy;edit.Enabled=healthy;delete.Enabled=healthy;done.Enabled=healthy;
   count.Text=healthy?String.Format("{0}  {1}개  |  미완료 {2}개 · 완료 {3}개 · 참고사항 {4}개",tabs.SelectedTab.Text,list.Items.Count,store.Data.Tasks.Count(t=>!t.Done),store.Data.Tasks.Count(t=>t.Done),store.Data.Notes.Count):"데이터 보호 모드 · 원본을 덮어쓰지 않습니다. 백업 복원으로 복구할 수 있습니다.";
   ShowPreview();
  }
  void ShowPreview() {
   var ids=SelectedIds();if(ids.Count==0){preview.Text="항목을 선택하면 상세내용이 여기에 표시됩니다.";return;}
   if(ids.Count>1){preview.Text=ids.Count+"개 항목 선택됨. 선택 완료 또는 삭제를 한 번에 실행할 수 있습니다.";return;}
   if(IsNotes){var n=store.Data.Notes.FirstOrDefault(x=>x.Id==ids[0]);if(n!=null)preview.Text=n.Title+"\r\n"+(String.IsNullOrEmpty(n.Tag)?"":n.Tag+"\r\n")+"\r\n"+n.Body;}
   else{var t=store.Data.Tasks.FirstOrDefault(x=>x.Id==ids[0]);if(t!=null)preview.Text=t.Title+"\r\n"+t.Category+(t.Done?" · 완료":" · 미완료")+(t.DueDate==""?"":" · 예정일 "+t.DueDate)+"\r\n\r\n"+t.Body;}
  }
  void Failure(Exception ex) {status.Text="변경을 저장하지 못했습니다.";MessageBox.Show(this,ex.Message+"\r\n\r\n기존 데이터는 유지됩니다. 저장 경로와 백업을 확인해 주세요.","작업 실패",MessageBoxButtons.OK,MessageBoxIcon.Error);}
  bool Persist(Action<AppData> action,string message,string id){try{store.Change(action);status.Text=message+" · "+DateTime.Now.ToString("HH:mm:ss");RefreshItems(id);return true;}catch(Exception ex){Failure(ex);return false;}}
  void EditItem(bool create) {
   if(store.LoadError!=null)return;
   var ids=SelectedIds();if(!create&&ids.Count!=1){status.Text="수정할 항목을 하나만 선택하세요.";return;}
   TaskItem task=null;NoteItem note=null;
   if(!create){if(IsNotes)note=store.Data.Notes.First(x=>x.Id==ids[0]);else task=store.Data.Tasks.First(x=>x.Id==ids[0]);}
   string group=IsNotes?(note==null?tabs.SelectedTab.Text:(note.Kind=="equipment"?"설비 참고사항":"업무 참고사항")):(task==null?CurrentCategory:task.Category);
   using(var dialog=new EditDialog(IsNotes,create?(IsNotes?"참고사항 추가":"업무 추가"):"항목 수정",task!=null?task.Title:note!=null?note.Title:"",task!=null?task.Body:note!=null?note.Body:"",group,task!=null?task.DueDate:"",note!=null?note.Tag:"")) {
    while(dialog.ShowDialog(this)==DialogResult.OK) {
     string title=dialog.ItemTitle,body=dialog.ItemBody,category=dialog.ItemCategory;
     bool success;
     if(IsNotes){string kind=category=="설비 참고사항"?"equipment":"work";var next=note==null?Model.NewNote(title,body,kind,dialog.ItemTag):Model.Serializer().Deserialize<NoteItem>(Model.Serializer().Serialize(note));next.Title=title;next.Body=body;next.Kind=kind;next.Tag=dialog.ItemTag;next.UpdatedAt=Model.Now();success=Persist(data=>{if(note!=null)data.Notes.RemoveAll(n=>n.Id==note.Id);data.Notes.Add(next);},"참고사항 저장됨",next.Id);}
     else{var next=task==null?Model.NewTask(title,body,category,dialog.ItemDue):Model.Serializer().Deserialize<TaskItem>(Model.Serializer().Serialize(task));next.Title=title;next.Body=body;next.Category=category;next.DueDate=dialog.ItemDue;next.UpdatedAt=Model.Now();success=Persist(data=>{if(task!=null)data.Tasks.RemoveAll(t=>t.Id==task.Id);data.Tasks.Add(next);},"업무 저장됨",next.Id);}
     if(success)break;
    }
   }
  }
  void CompleteSelected() {
   var ids=SelectedIds();if(ids.Count==0){status.Text="완료 처리할 업무를 선택하세요.";return;}
   bool complete=tabs.SelectedIndex!=3;Persist(data=>Model.SetDone(data,ids,complete),complete?ids.Count+"개 업무 완료됨":ids.Count+"개 업무 완료 취소됨",null);
  }
  void DeleteSelected() {
   var ids=SelectedIds();if(ids.Count==0){status.Text="삭제할 항목을 선택하세요.";return;}
   if(MessageBox.Show(this,ids.Count+"개 항목을 삭제할까요?\r\n삭제 전 데이터는 자동 백업됩니다.","삭제 확인",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
   Persist(data=>{if(IsNotes)data.Notes.RemoveAll(n=>ids.Contains(n.Id));else data.Tasks.RemoveAll(t=>ids.Contains(t.Id));},"선택 항목 삭제됨",null);
  }
  void ExportBackup(){using(var dialog=new SaveFileDialog{Title="업무와 참고사항 백업 저장",Filter="업무 백업 (*.json)|*.json",FileName="업무백업-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".json",OverwritePrompt=true}){if(dialog.ShowDialog(this)!=DialogResult.OK)return;try{store.Export(dialog.FileName);status.Text="백업 저장 완료 · "+dialog.FileName;}catch(Exception ex){Failure(ex);}}}
  void RestoreBackup(){using(var dialog=new OpenFileDialog{Title="이 프로그램에서 만든 백업 선택",Filter="업무 백업 (*.json)|*.json"}){if(dialog.ShowDialog(this)!=DialogResult.OK)return;try{var candidate=DataStore.Read(dialog.FileName);if(MessageBox.Show(this,String.Format("업무 {0}개, 참고사항 {1}개로 전체 기록을 복원할까요?\r\n현재 파일은 교체 전에 자동 백업됩니다.",candidate.Tasks.Count,candidate.Notes.Count),"복원 확인",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;store.Restore(dialog.FileName);status.Text="백업 복원 완료";RefreshItems(null);}catch(Exception ex){Failure(ex);}}}
  void ImportLegacy(){using(var dialog=new OpenFileDialog{Title="v0.1.0 할일데이터.json 선택",Filter="기존 할 일 목록 (*.json)|*.json"}){if(dialog.ShowDialog(this)!=DialogResult.OK)return;try{var candidate=Model.DecodeLegacy(DataStore.ReadText(dialog.FileName));if(MessageBox.Show(this,candidate.Tasks.Count+"개 항목을 금일사항으로 추가할까요?\r\n완료된 항목은 완료 목록으로 들어갑니다. 반복 가져오면 중복 추가됩니다.","기존 목록 가져오기",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;store.ImportLegacy(dialog.FileName);status.Text="기존 목록 가져오기 완료 · 원본 파일 보존";RefreshItems(null);}catch(Exception ex){Failure(ex);}}}
 }
}



using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WorkTodo {
 public static class Tests {
  static readonly List<string> Results=new List<string>();
  static int failed;
  static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
  static void Reject(Action action){bool threw=false;try{action();}catch{threw=true;}Check(threw,"Expected operation to be rejected");}
  static void Run(string name,Action test){try{test();Results.Add("PASS " + name);}catch(Exception ex){failed++;Results.Add("FAIL "+name+": "+ex);}}
  public static int Execute(string report) {
   string root=Path.Combine(Path.GetTempPath(),"WorkTodoTests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
   Run("Three task categories and two note types",delegate{
    var data=new AppData();foreach(var category in Model.Categories)data.Tasks.Add(Model.NewTask(category,"상세\r\n두 번째 줄",category,"2026-10-24"));
    data.Notes.Add(Model.NewNote("설비 예시","본문","equipment","설비 A"));data.Notes.Add(Model.NewNote("업무 예시","본문","work","절차"));
    var result=Model.Decode(Model.Encode(data));Check(result.Tasks.Count==3&&result.Notes.Count==2,"Count");Check(result.Tasks[0].Body.Contains("\r\n"),"Unicode/newlines");
   });
   Run("Selected completion and cancellation preserve category, note and history",delegate{
    var data=new AppData();var a=Model.NewTask("A","내용","진행사항","");var b=Model.NewTask("B","","예정사항","");data.Tasks.Add(a);data.Tasks.Add(b);data.Notes.Add(Model.NewNote("N","","work",""));
    Model.SetDone(data,new[]{a.Id},true);Check(a.Done&&!b.Done&&a.CompletedAt!=""&&a.Category=="진행사항"&&data.Notes.Count==1,"Complete");
    string completed=a.CompletedAt;Model.SetDone(data,new[]{a.Id},true);Check(a.CompletedAt==completed,"Idempotent");
    Model.SetDone(data,new[]{a.Id},false);Check(!a.Done&&a.CompletedAt==""&&a.Title=="A","Cancel");
   });
   Run("Whitespace, title and body limits",delegate{Reject(()=>Model.NewTask("   ","","금일사항",""));Reject(()=>Model.NewTask(new string('가',201),"","금일사항",""));Reject(()=>Model.NewTask("A",new string('가',40001),"금일사항",""));Check(Model.NewTask(new string('가',200),"","금일사항","").Title.Length==200,"Boundary");});
   Run("Invalid schema, dates, categories, duplicate IDs and incomplete records rejected",delegate{
    Reject(()=>Model.Decode("{}"));Reject(()=>Model.Decode("{\"SchemaVersion\":99,\"Tasks\":[],\"Notes\":[]}"));
    var data=new AppData();var t=Model.NewTask("A","","금일사항","");data.Tasks.Add(t);data.Tasks.Add(t);Reject(()=>Model.Validate(data));data.Tasks.RemoveAt(1);
    t.DueDate="2026-02-30";Reject(()=>Model.Validate(data));t.DueDate="";t.Category="unknown";Reject(()=>Model.Validate(data));
    Reject(()=>Model.Decode("{\"SchemaVersion\":2,\"Tasks\":[{\"Title\":\"A\"}],\"Notes\":[]}"));
   });
   Run("Save, reopen and edit persist without deleting unrelated entries",delegate{
    string folder=Path.Combine(root,"persist");using(var store=new DataStore(folder,null)){
     store.Change(d=>{d.Tasks.Add(Model.NewTask("업무","첫 내용","금일사항",""));d.Notes.Add(Model.NewNote("참고","원본","equipment","A"));});
     store.Change(d=>{d.Tasks[0].Body="수정 내용";d.Tasks[0].Category="진행사항";d.Tasks[0].UpdatedAt=Model.Now();});
    }
    using(var store=new DataStore(folder,null)){Check(store.Data.Tasks[0].Body=="수정 내용"&&store.Data.Tasks[0].Category=="진행사항"&&store.Data.Notes.Count==1,"Persistence");}
   });
   Run("Multiple selected task completion and independent note deletion",delegate{
    var data=new AppData();for(int i=0;i<3;i++)data.Tasks.Add(Model.NewTask("Task "+i,"","금일사항",""));data.Notes.Add(Model.NewNote("note","","work",""));
    Model.SetDone(data,data.Tasks.Take(2).Select(t=>t.Id),true);Check(data.Tasks.Count(t=>t.Done)==2,"Batch complete");data.Notes.Clear();Check(data.Tasks.Count==3,"Note isolation");
   });
   Run("Backup export and restore with previous-file automatic backup",delegate{
    using(var store=new DataStore(Path.Combine(root,"restore"),null)){
     store.Change(d=>d.Tasks.Add(Model.NewTask("before","","금일사항","")));string backup=Path.Combine(root,"export.json");store.Export(backup);
     store.Change(d=>d.Tasks.Clear());store.Restore(backup);Check(store.Data.Tasks.Count==1,"Restore");
     Check(Directory.GetFiles(Path.Combine(store.Folder,"backups"),"*.json").Length>=2,"Automatic backup");
     Reject(()=>store.Export(store.DataPath));
    }
   });
   Run("Invalid restore does not overwrite existing data",delegate{
    using(var store=new DataStore(Path.Combine(root,"invalid-restore"),null)){
     store.Change(d=>d.Tasks.Add(Model.NewTask("safe","","금일사항","")));string before=File.ReadAllText(store.DataPath);string bad=Path.Combine(root,"bad.json");File.WriteAllText(bad,"{");Reject(()=>store.Restore(bad));Check(File.ReadAllText(store.DataPath)==before&&store.Data.Tasks.Count==1,"Preserve data");
    }
   });
   Run("Corrupt startup is protected and recoverable using valid backup",delegate{
    string folder=Path.Combine(root,"corrupt");Directory.CreateDirectory(folder);string path=Path.Combine(folder,"업무데이터.json");File.WriteAllText(path,"corrupted");
    using(var store=new DataStore(folder,null)){
     Check(store.LoadError!=null,"Protection");Reject(()=>store.Change(d=>d.Tasks.Clear()));Check(File.ReadAllText(path)=="corrupted","Not overwritten");
     string backup=Path.Combine(root,"valid.json");File.WriteAllText(backup,Model.Encode(new AppData()));store.Restore(backup);Check(store.LoadError==null&&store.Data.Tasks.Count==0,"Recovered");
     Check(Directory.GetFiles(Path.Combine(folder,"backups")).Any(p=>File.ReadAllText(p)=="corrupted"),"Original preserved");
    }
   });
   Run("Failed disk write keeps previous memory and disk data",delegate{
    using(var store=new DataStore(Path.Combine(root,"write-failure"),null)){
     store.Change(d=>d.Tasks.Add(Model.NewTask("safe","","금일사항","")));string before=File.ReadAllText(store.DataPath);
     using(var locked=new FileStream(store.DataPath,FileMode.Open,FileAccess.ReadWrite,FileShare.None)){Reject(()=>store.Change(d=>d.Tasks.Clear()));}
     Check(store.Data.Tasks.Count==1&&File.ReadAllText(store.DataPath)==before,"Rollback");Check(Directory.GetFiles(store.Folder,"*.tmp").Length==0,"Temporary file cleanup");
    }
   });
   Run("Second instance cannot open the same data directory",delegate{
    string folder=Path.Combine(root,"locking");using(var store=new DataStore(folder,null)){Reject(()=>{using(var second=new DataStore(folder,null)){};});}
    using(var reopened=new DataStore(folder,null)){Check(reopened.LoadError==null,"Release lock");}
   });
   Run("Legacy import preserves original and completed records",delegate{
    string old=Path.Combine(root,"legacy.json");const string text="[{\"Title\":\"이전 업무\",\"Done\":true}]";File.WriteAllText(old,text);
    string folder=Path.Combine(root,"migration");using(var store=new DataStore(folder,old)){
     Check(store.ImportedLegacy&&store.Data.Tasks.Count==1&&store.Data.Tasks[0].Done&&store.Data.Tasks[0].Category=="금일사항","Migration");Check(File.ReadAllText(old)==text,"Legacy unchanged");store.ImportLegacy(old);Check(store.Data.Tasks.Count==2,"Append import");
    }
    using(var store=new DataStore(folder,old)){Check(store.Data.Tasks.Count==2&&!store.ImportedLegacy,"No duplicate automatic migration");}
   });
   Run("Broken legacy input stays unchanged without creating a new database",delegate{
    string old=Path.Combine(root,"legacy-bad.json");File.WriteAllText(old,"[{}]");using(var store=new DataStore(Path.Combine(root,"legacy-bad"),old)){Check(store.LoadError!=null&&!File.Exists(store.DataPath),"Reject broken legacy");}
   });
   Run("Native window and editors create with six tabs",delegate{
    using(var store=new DataStore(Path.Combine(root,"ui"),null))using(var form=new MainWindow(store,false)){
     form.Show();Application.DoEvents();var tab=Descendants(form).OfType<TabControl>().Single();Check(tab.TabPages.Count==6,"Six tabs");
     tab.SelectedIndex=4;Application.DoEvents();Check(Descendants(form).OfType<Button>().Any(b=>b.Text=="+ 새 참고사항"),"Note toolbar");form.Close();
    }
    using(var edit=new EditDialog(false,"test","A","B","예정사항","2026-10-24","")){Check(edit.ItemTitle=="A"&&edit.ItemCategory=="예정사항"&&edit.ItemDue=="2026-10-24","Task editor");}
    using(var edit=new EditDialog(true,"test","N","B","설비 참고사항","","A")){Check(edit.ItemTag=="A","Note editor");}
   });
   Results.Add(String.Format("TOTAL {0}; PASS {1}; FAIL {2}",Results.Count,Results.Count-failed,failed));
   Results.Add("Actual mouse and keyboard testing and target intranet verification are separate checks.");
   File.WriteAllLines(report,Results,new UTF8Encoding(false));return failed==0?0:1;
  }
  static IEnumerable<Control> Descendants(Control root){foreach(Control child in root.Controls){yield return child;foreach(var nested in Descendants(child))yield return nested;}}
 }
}


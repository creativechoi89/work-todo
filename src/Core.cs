using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace WorkTodo {
 public class TaskItem {
  public string Id { get; set; }
  public string Title { get; set; }
  public string Body { get; set; }
  public string Category { get; set; }
  public string DueDate { get; set; }
  public bool Done { get; set; }
  public string CreatedAt { get; set; }
  public string UpdatedAt { get; set; }
  public string CompletedAt { get; set; }
 }
 public class NoteItem {
  public string Id { get; set; }
  public string Kind { get; set; }
  public string Title { get; set; }
  public string Body { get; set; }
  public string Tag { get; set; }
  public string CreatedAt { get; set; }
  public string UpdatedAt { get; set; }
 }
 public class AppData {
  public int SchemaVersion { get; set; }
  public List<TaskItem> Tasks { get; set; }
  public List<NoteItem> Notes { get; set; }
  public AppData() { SchemaVersion = 2; Tasks = new List<TaskItem>(); Notes = new List<NoteItem>(); }
 }
 public static class Model {
  public static readonly string[] Categories = { "금일사항", "진행사항", "예정사항" };
  public static string Now() { return DateTime.Now.ToString("o", CultureInfo.InvariantCulture); }
  public static string CleanTitle(string title) {
   title = (title ?? "").Trim();
   if (title.Length == 0 || title.Length > 200) throw new InvalidDataException("제목은 공백을 제외하고 1~200자로 입력해 주세요.");
   return title;
  }
  public static string CleanBody(string body) {
   body = body ?? "";
   if (body.Length > 40000) throw new InvalidDataException("내용은 40,000자 이내로 입력해 주세요.");
   return body;
  }
  public static TaskItem NewTask(string title, string body, string category, string due) {
   string now = Now();
   return new TaskItem { Id = Guid.NewGuid().ToString("N"), Title = CleanTitle(title), Body = CleanBody(body), Category = category, DueDate = due ?? "", CreatedAt = now, UpdatedAt = now, CompletedAt = "", Done = false };
  }
  public static NoteItem NewNote(string title, string body, string kind, string tag) {
   string now = Now();
   return new NoteItem {Id = Guid.NewGuid().ToString("N"), Title = CleanTitle(title), Body = CleanBody(body), Kind = kind, Tag = (tag ?? "").Trim(), CreatedAt = now, UpdatedAt = now};
  }
  public static JavaScriptSerializer Serializer() { return new JavaScriptSerializer { MaxJsonLength = 32 * 1024 * 1024, RecursionLimit = 60 }; }
  public static string Encode(AppData data) { return Serializer().Serialize(data); }
  public static AppData Decode(string text) {
   if (String.IsNullOrWhiteSpace(text)) throw new InvalidDataException("비어 있는 데이터 파일입니다.");
   var raw = Serializer().DeserializeObject(text) as Dictionary<string,object>;
   if (raw == null || !raw.ContainsKey("SchemaVersion") || !raw.ContainsKey("Tasks") || !raw.ContainsKey("Notes")) throw new InvalidDataException("지원하는 업무 데이터 파일이 아닙니다.");
   // Reject incomplete records rather than silently defaulting missing fields.
   RequireFields(raw["Tasks"], new [] {"Id","Title","Body","Category","DueDate","Done","CreatedAt","UpdatedAt","CompletedAt"});
   RequireFields(raw["Notes"], new [] {"Id","Kind","Title","Body","Tag","CreatedAt","UpdatedAt"});
   AppData data = Serializer().Deserialize<AppData>(text); Validate(data); return data;
  }
  static void RequireFields(object value, string[] names) {
   var array = value as object[];
   if (array == null) throw new InvalidDataException("목록 형식이 올바르지 않습니다.");
   foreach (object item in array) {
    var record = item as Dictionary<string,object>;
    if (record == null || names.Any(n => !record.ContainsKey(n))) throw new InvalidDataException("필수 데이터 항목이 누락되었습니다.");
    foreach (string name in names) {
     if (name == "Done") { if (!(record[name] is bool)) throw new InvalidDataException("완료 상태 형식 오류"); }
     else if (!(record[name] is string)) throw new InvalidDataException("문자 데이터 형식 오류");
    }
   }
  }
  static bool IsTimestamp(string value) {
   DateTimeOffset parsed;
   return !String.IsNullOrEmpty(value) && DateTimeOffset.TryParseExact(value,"o",CultureInfo.InvariantCulture,DateTimeStyles.None,out parsed);
  }
  public static void Validate(AppData data) {
   if (data == null || data.SchemaVersion != 2 || data.Tasks == null || data.Notes == null) throw new InvalidDataException("데이터 버전을 지원하지 않습니다. 현재 버전: 2");
   var ids = new HashSet<string>();
   foreach (TaskItem t in data.Tasks) {
    DateTime date;
    if (t == null || String.IsNullOrWhiteSpace(t.Id) || !ids.Add(t.Id) || CleanTitle(t.Title) != t.Title || t.Body == null || CleanBody(t.Body) != t.Body || !Categories.Contains(t.Category) || t.DueDate == null || (t.DueDate != "" && !DateTime.TryParseExact(t.DueDate,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out date)) || !IsTimestamp(t.CreatedAt) || !IsTimestamp(t.UpdatedAt) || t.CompletedAt == null || (t.Done ? !IsTimestamp(t.CompletedAt) : t.CompletedAt != "")) throw new InvalidDataException("업무 기록이 올바르지 않습니다.");
   }
   foreach (NoteItem n in data.Notes) {
    if (n == null || String.IsNullOrWhiteSpace(n.Id) || !ids.Add(n.Id) || CleanTitle(n.Title) != n.Title || n.Body == null || CleanBody(n.Body) != n.Body || (n.Kind != "equipment" && n.Kind != "work") || n.Tag == null || n.Tag.Length > 200 || !IsTimestamp(n.CreatedAt) || !IsTimestamp(n.UpdatedAt)) throw new InvalidDataException("참고사항 기록이 올바르지 않습니다.");
   }
  }
  public static AppData Clone(AppData data) { return Decode(Encode(data)); }
  public static void SetDone(AppData data, IEnumerable<string> ids, bool done) {
   var selected = new HashSet<string>(ids); string now = Now();
   foreach (var item in data.Tasks.Where(t => selected.Contains(t.Id))) {
    if (item.Done == done) continue;
    item.Done = done; item.CompletedAt = done ? now : ""; item.UpdatedAt = now;
   }
  }
  public static AppData DecodeLegacy(string text) {
   var array = Serializer().DeserializeObject(text) as object[];
   if (array == null) throw new InvalidDataException("기존 할 일 목록 파일이 아닙니다.");
   var data = new AppData();
   foreach (object value in array) {
    var old = value as Dictionary<string,object>;
    if (old == null || !old.ContainsKey("Title") || !old.ContainsKey("Done") || !(old["Title"] is string) || !(old["Done"] is bool)) throw new InvalidDataException("기존 데이터 형식이 잘못되었습니다.");
    var t = NewTask((string)old["Title"],"","금일사항","");
    t.Done = (bool)old["Done"]; t.CompletedAt = t.Done ? Now() : ""; data.Tasks.Add(t);
   }
   Validate(data); return data;
  }
 }
 public class DataStore : IDisposable {
  public string Folder { get; private set; }
  public string DataPath { get; private set; }
  public AppData Data { get; private set; }
  public string LoadError { get; private set; }
  public bool ImportedLegacy { get; private set; }
  private FileStream gate;
  public DataStore(string folder, string legacyPath) {
   Folder = Path.GetFullPath(folder); Directory.CreateDirectory(Folder);
   gate = new FileStream(Path.Combine(Folder,"session.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
   DataPath = Path.Combine(Folder,"업무데이터.json"); Data = new AppData();
   try {
    if (File.Exists(DataPath)) Data = Read(DataPath);
    else if (!String.IsNullOrEmpty(legacyPath) && File.Exists(legacyPath)) {
     var migrated = Model.DecodeLegacy(ReadText(legacyPath));
     Directory.CreateDirectory(Path.Combine(Folder,"backups"));
     File.Copy(legacyPath,BackupPath("legacy"));
     Commit(migrated); ImportedLegacy = true;
    }
   } catch (Exception ex) { LoadError = "기록을 읽을 수 없습니다. 원본을 보존했습니다. 백업 복원을 이용하세요.\r\n" + ex.Message; }
  }
  public static string ReadText(string path) {
   if (new FileInfo(path).Length > 32 * 1024 * 1024) throw new InvalidDataException("데이터 파일이 32MB를 초과했습니다.");
   return File.ReadAllText(path,new UTF8Encoding(false,true));
  }
  public static AppData Read(string path) { return Model.Decode(ReadText(path)); }
  string BackupPath(string reason) { return Path.Combine(Folder,"backups",reason + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fffffff") + "-" + Guid.NewGuid().ToString("N").Substring(0,6) + ".json"); }
  public void Change(Action<AppData> mutation) {
   if (LoadError != null) throw new InvalidOperationException(LoadError);
   AppData next = Model.Clone(Data); mutation(next); Commit(next);
  }
  void Commit(AppData next) {
   Model.Validate(next);
   string text = Model.Encode(next);
   if (Encoding.UTF8.GetByteCount(text) > 32 * 1024 * 1024) throw new InvalidDataException("데이터가 32MB를 초과했습니다. 저장하지 않았습니다.");
   string temp = Path.Combine(Folder,Guid.NewGuid().ToString("N") + ".tmp");
   try {
    using (var stream = new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)) {
     byte[] bytes = new UTF8Encoding(false).GetBytes(text); stream.Write(bytes,0,bytes.Length); stream.Flush(true);
    }
    if (File.Exists(DataPath)) {
     Directory.CreateDirectory(Path.Combine(Folder,"backups"));
     File.Copy(DataPath,BackupPath("before-save"));
     File.Replace(temp,DataPath,null);
    } else File.Move(temp,DataPath);
    Data = next; LoadError = null;
   } finally { if (File.Exists(temp)) File.Delete(temp); }
  }
  public void Export(string path) {
   if (LoadError != null) throw new InvalidOperationException(LoadError);
   if (String.Equals(Path.GetFullPath(path),DataPath,StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("원본 데이터와 다른 위치에 백업해 주세요.");
   string temp = Path.GetFullPath(path) + "." + Guid.NewGuid().ToString("N") + ".tmp";
   try {
    File.WriteAllText(temp,Model.Encode(Data),new UTF8Encoding(false));
    Read(temp);
    if (File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path);
   } finally {if (File.Exists(temp)) File.Delete(temp);}
  }
  public void Restore(string path) { AppData restored = Read(path); Commit(restored); }
  public void ImportLegacy(string path) {
   if (LoadError != null) throw new InvalidOperationException(LoadError);
   AppData legacy = Model.DecodeLegacy(ReadText(path));
   Change(data => data.Tasks.AddRange(legacy.Tasks));
  }
  public void Dispose() { if (gate != null) {gate.Dispose();gate=null;} }
 }
}


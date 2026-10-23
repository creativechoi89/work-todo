using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
[assembly: AssemblyTitle("할일관리")]
[assembly: AssemblyVersion("0.3.0.0")]
[assembly: AssemblyFileVersion("0.3.0.0")]
namespace WorkTodo {
 static class Program {
  [STAThread] static int Main(string[] args) {
   Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
   Application.ThreadException+=delegate(object sender,System.Threading.ThreadExceptionEventArgs e){MessageBox.Show(e.Exception.Message,"예기치 않은 오류",MessageBoxButtons.OK,MessageBoxIcon.Error);};
   string folder=AppDomain.CurrentDomain.BaseDirectory;
   if(args.Length>0&&args[0]=="--self-test")return Tests.Execute(args.Length>1?Path.GetFullPath(args[1]):Path.Combine(folder,"test-results.txt"));
   bool demo=args.Length>0&&args[0]=="--preview";
   string dataFolder=demo?Path.Combine(Path.GetTempPath(),"WorkTodoPreview-"+Guid.NewGuid().ToString("N")):Path.Combine(folder,"data");
   try {
    using(var store=new DataStore(dataFolder,demo?null:Path.Combine(folder,"할일데이터.json"))){
     if(demo)store.Change(data=>{
      data.Tasks.Add(Model.NewTask("금일 점검 내용 정리","예시 데이터입니다. 실제 업무 기록과 분리되어 있습니다.\r\n점검 후 확인 내용을 기록해 보세요.","금일사항",DateTime.Today.ToString("yyyy-MM-dd")));
      data.Tasks.Add(Model.NewTask("진행 중인 업무 확인","다음 진행 단계를 적어 보세요.","진행사항",""));
      data.Tasks.Add(Model.NewTask("다음 주 일정 준비","예정일은 선택 입력할 수 있습니다.","예정사항",DateTime.Today.AddDays(3).ToString("yyyy-MM-dd")));
      var complete=Model.NewTask("완료한 업무 예시","완료 취소하면 원래 분류로 돌아갑니다.","금일사항","");data.Tasks.Add(complete);Model.SetDone(data,new[]{complete.Id},true);
      data.Notes.Add(Model.NewNote("설비 점검 참고 예시","1. 점검 전 상태 확인\r\n2. 점검 내용 기록\r\n3. 결과 확인","equipment","설비 A (예시)"));
      data.Notes.Add(Model.NewNote("업무 인수인계 참고 예시","업무 절차와 참고 내용을 자유롭게 작성할 수 있습니다.","work","인수인계 (예시)"));
     });
     Application.Run(new MainWindow(store,demo));
    }
    return 0;
   } catch(Exception ex){MessageBox.Show("프로그램을 시작할 수 없습니다.\r\n같은 폴더의 프로그램이 이미 열려 있는지, 폴더에 쓰기 권한이 있는지 확인하세요.\r\n\r\n"+ex.Message,"할일관리",MessageBoxButtons.OK,MessageBoxIcon.Error);return 1;}
  }
 }
}


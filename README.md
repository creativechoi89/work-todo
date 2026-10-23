# 할일 프로젝트
사내망 Windows PC에서 오프라인으로 사용하는 독립 창 업무 관리 프로그램.

- 개발 기준: AGENTS.md
- 구현/검증 상태 및 남은 작업: STATUS.md
- 요구사항 변경 기록: CHANGELOG.md
- 현재 예제 소스: TodoApp.cs
- 현재 실행 파일: 할일관리.exe (로컬 빌드 결과)
- 실행 및 기존 검사 설명: 사용안내.md

## 다시 빌드
PowerShell에서 프로젝트 폴더로 이동한 뒤:
```powershell
.\build.ps1
```
실행 중인 프로그램이 있다면 닫고 빌드한다. 현재는 Windows 내장 .NET Framework C# 컴파일러를 사용한다.
대상 PC에서 프로그램을 실행하는 데 이 빌드 스크립트나 컴파일러는 필요하지 않다.

## 보존
소스와 문서는 전용 공개 GitHub 저장소에 보존한다. 실제 업무 데이터와 참고사항은 업로드하지 않는다.
저장소가 정해지고 원격 파일과 커밋을 확인하기 전까지 원격 백업은 미완료다.
현재 HTML/JavaScript 파일들은 초기 웹 예제 기록이며 Windows 앱 기능의 기준은 TodoApp.cs다.


## ZIP 바로 다운로드
- 실행용 ZIP: https://github.com/creativechoi89/work-todo/releases/download/v0.1.0/work-todo-windows.zip
- 소스 보존 ZIP: https://github.com/creativechoi89/work-todo/releases/download/v0.1.0/work-todo-source.zip

실행용 ZIP을 별도 폴더에 압축 해제하고 할일관리.exe를 실행한다. 현재 버전은 단일 목록 예제이며 금일/진행/예정 분류와 참고사항 화면은 아직 구현되지 않았다. 사내망 PC 호환성은 미확인이다.
앞으로도 업로드 결과는 릴리스 첨부 ZIP과 직접 다운로드 링크로 제공한다.

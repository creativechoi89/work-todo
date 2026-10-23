# 할일관리
사내망 Windows PC에서 인터넷 없이 사용하는 독립 실행형 업무 관리 프로그램.

## 바로 다운로드
- [실행용 ZIP](https://github.com/creativechoi89/work-todo/releases/download/v0.3.0/work-todo-windows.zip)
- [소스 보존용 ZIP](https://github.com/creativechoi89/work-todo/releases/download/v0.3.0/work-todo-source.zip)

새 폴더에 압축을 풀고 할일관리.exe를 실행합니다. Windows 및 .NET Framework 4.x가 필요합니다. 대상 사내망 PC 호환성은 미확인입니다.

## 달력
첫 화면 오른쪽 위 월간 달력에서 날짜를 클릭하면 예정일 기준으로 현재 업무 목록을 조회합니다. 업무가 있는 날은 굵게 표시하고, 왼쪽에 전체 분류의 선택 날짜 업무 수를 표시합니다.
전체 날짜 보기로 예정일 없는 업무까지 복구합니다. 참고사항은 날짜 제한 없이 표시합니다. 날짜를 선택한 상태에서 업무를 추가하면 예정일이 해당 날짜로 기본 입력됩니다.

## 기능
- 업무 3개 분류, 완료 목록과 완료 취소, 여러 업무 선택 처리
- 업무 제목·상세내용·분류·선택 예정일 편집
- 설비 참고사항 / 업무 참고사항 별도 작성·편집 및 검색
- 로컬 자동 저장, 저장 전 백업, JSON 백업/복원
- 이전 v0.1.0 데이터 가져오기, 같은 데이터 폴더 동시 실행 차단

사용 방법은 사용안내.md, 개발 기준은 AGENTS.md, 검증 상태는 STATUS.md를 확인하세요.

## 개발
소스는 src/에 있습니다. Windows PowerShell에서 프로젝트 폴더의 build.ps1을 실행하면 .NET Framework C# 컴파일러로 빌드하며 bin/할일관리.exe에 저장합니다. 해당 bin 프로그램이 열려 있으면 닫고 빌드하세요. 이전 프로젝트 루트의 exe는 자동 교체하지 않습니다.
자동 검사: bin/할일관리.exe --self-test (test-results.txt에 결과 기록, 임시 폴더의 가상 데이터 사용).
미리보기: bin/할일관리.exe --preview (별도 임시 폴더에 예시 업무와 참고사항 생성).
사용자 데이터 data/와 이전 할일데이터.json은 저장소/배포본에서 제외합니다.


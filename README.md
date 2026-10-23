# 할일관리
사내망 Windows PC에서 인터넷 없이 사용하는 독립 실행형 업무 관리 프로그램.

## 바로 다운로드
- [실행용 ZIP](https://github.com/creativechoi89/work-todo/releases/download/v0.2.0/work-todo-windows.zip)
- [소스 보존용 ZIP](https://github.com/creativechoi89/work-todo/releases/download/v0.2.0/work-todo-source.zip)

새 폴더에 압축을 풀고 할일관리.exe를 실행합니다. Windows 및 .NET Framework 4.x가 필요합니다. 대상 사내망 PC 호환성은 미확인입니다.

## 기능
- 업무 3개 분류, 완료 목록과 완료 취소, 여러 업무 선택 처리
- 업무 제목·상세내용·분류·선택 예정일 편집
- 설비 참고사항 / 업무 참고사항 별도 작성·편집 및 검색
- 로컬 자동 저장, 저장 전 백업, JSON 백업/복원
- 이전 v0.1.0 데이터 가져오기, 같은 데이터 폴더 동시 실행 차단

사용 방법은 사용안내.md, 개발 기준은 AGENTS.md, 검증 상태는 STATUS.md를 확인하세요.

## 개발
소스는 src/에 있습니다. Windows PowerShell에서 프로젝트 폴더의 build.ps1을 실행하면 .NET Framework C# 컴파일러로 빌드합니다. 실행 중인 프로그램을 닫고 빌드하세요.
자동 검사: 할일관리.exe --self-test (test-results.txt에 결과 기록, 임시 폴더의 가상 데이터 사용).
미리보기: 할일관리.exe --preview (별도 임시 폴더에 예시 업무와 참고사항 생성).
사용자 데이터 data/와 이전 할일데이터.json은 저장소/배포본에서 제외합니다.

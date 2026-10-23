# 검사 결과 — v0.2.0

2026-10-23 개발 PC에서 빌드 후 실행한 결과입니다.

```text
PASS Three task categories and two note types
PASS Selected completion and cancellation preserve category, note and history
PASS Whitespace, title and body limits
PASS Invalid schema, dates, categories, duplicate IDs and incomplete records rejected
PASS Save, reopen and edit persist without deleting unrelated entries
PASS Multiple selected task completion and independent note deletion
PASS Backup export and restore with previous-file automatic backup
PASS Invalid restore does not overwrite existing data
PASS Corrupt startup is protected and recoverable using valid backup
PASS Failed disk write keeps previous memory and disk data
PASS Second instance cannot open the same data directory
PASS Legacy import preserves original and completed records
PASS Broken legacy input stays unchanged without creating a new database
PASS Native window and editors create with six tabs
TOTAL 14; PASS 14; FAIL 0
Actual mouse and keyboard testing and target intranet verification are separate checks.
```

## 화면 확인
실제 사용자 Windows 세션에서 기본 창과 업무 추가 창을 열고 화면을 확인했습니다.
입력 자동화는 사용자 입력 감지로 중단되었습니다. 전체 마우스/키보드 인수 검사는 완료하지 않았습니다.

## 사용자 인수 검사 체크리스트
- [ ] 금일/진행/예정 각각 추가, 여러 줄 내용 수정, 분류 이동
- [ ] 여러 항목 선택 완료, 완료 목록 조회, 완료 취소
- [ ] 설비/업무 참고사항 각각 추가·수정·삭제 및 검색
- [ ] 종료/재실행 후 기록과 완료 상태 유지
- [ ] 백업 저장 후 새 폴더의 프로그램에서 전체 복원
- [ ] 이전 v0.1.0 파일 가져오기와 원본 보존 확인
- [ ] 사내망 PC에서 인터넷 없이 실행·저장·재실행·복원

자동 검사는 실제 저장 파일 및 WinForms 생성으로 실행했고 입력 자동화와 구분합니다.
완료 기준: 자동 검사 통과 + 위 사용자 인수 검사 통과. 현재는 자동 검사와 화면 확인까지 완료했습니다.

## 배포본 확인
실행용 ZIP을 별도 작업 폴더에 압축 해제하여 내부 파일 SHA-256을 검증하고, 추출한 exe에서 자동 검사 14개 재실행을 통과했습니다.


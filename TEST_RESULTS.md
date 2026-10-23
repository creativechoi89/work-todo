# v0.3.0 검사 결과

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
PASS Calendar dates include completed work and deduplicate while ignoring undated work
PASS Calendar filter updates real lists, completed view, notes and reset without changing saved data
TOTAL 16; PASS 16; FAIL 0
Actual mouse and keyboard testing and target intranet verification are separate checks.

실제 Windows 달력 화면, 날짜 클릭 시 필터 켜짐, 전체 날짜 보기 해제를 확인했다.
전체 기존 기능의 수동 인수 검사와 사내망 PC 검증은 미완료다.


'use strict';
const KEY = 'control-test-todos-v1';
const core = window.TodoCore;
const list = document.querySelector('#tasks');
const input = document.querySelector('#task');
const status = document.querySelector('#status');
let tasks = [];
let storageReady = true;
try { tasks = core.decode(localStorage.getItem(KEY)); }
catch (_) {
  storageReady = false;
  status.textContent = '저장된 기록을 읽을 수 없습니다. 기존 기록을 보호하기 위해 이번 변경은 메모리에만 유지됩니다.';
}
function save() {
  if (!storageReady) return;
  try { localStorage.setItem(KEY, JSON.stringify(tasks)); status.textContent = '저장되었습니다.'; }
  catch (_) { status.textContent = '저장할 수 없습니다. 현재 화면에서는 사용할 수 있지만 새로고침하면 변경이 사라질 수 있습니다.'; }
}
function render() {
  list.replaceChildren();
  for (const task of tasks) {
    const li = document.createElement('li');
    const label = document.createElement('label');
    const check = document.createElement('input');
    check.type = 'checkbox'; check.checked = task.done;
    const title = document.createElement('span');
    title.textContent = task.title;
    if (task.done) title.className = 'done';
    check.addEventListener('change', () => { tasks = core.toggle(tasks, task.id); save(); render(); });
    label.append(check, title);
    const button = document.createElement('button');
    button.type = 'button'; button.className = 'delete'; button.textContent = '삭제';
    button.setAttribute('aria-label', task.title + ' 삭제');
    button.addEventListener('click', () => { tasks = core.remove(tasks, task.id); save(); render(); });
    li.append(label, button); list.append(li);
  }
  document.querySelector('#summary').textContent = `전체 ${tasks.length}개 · 완료 ${tasks.filter(t => t.done).length}개`;
  document.querySelector('#empty').hidden = tasks.length > 0;
}
document.querySelector('#add-form').addEventListener('submit', event => {
  event.preventDefault();
  try {
    const id = Date.now().toString(36) + '-' + Math.random().toString(36).slice(2);
    tasks = core.add(tasks, input.value, id);
    save(); render(); input.value = ''; input.focus();
  } catch (error) { status.textContent = error.message; }
});
render();

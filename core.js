(function (root) {
  'use strict';
  function normalizeTitle(value) {
    if (typeof value !== 'string') throw new Error('할 일을 입력해 주세요.');
    const title = value.trim();
    if (!title) throw new Error('공백만 입력할 수 없습니다.');
    if (title.length > 120) throw new Error('120자 이내로 입력해 주세요.');
    return title;
  }
  function add(tasks, title, id) {
    if (typeof id !== 'string' || !id || tasks.some(t => t.id === id)) throw new Error('잘못된 ID입니다.');
    return [...tasks, { id, title: normalizeTitle(title), done: false }];
  }
  function toggle(tasks, id) {
    return tasks.map(t => t.id === id ? { ...t, done: !t.done } : t);
  }
  function remove(tasks, id) { return tasks.filter(t => t.id !== id); }
  function decode(raw) {
    if (raw === null) return [];
    const tasks = JSON.parse(raw);
    if (!Array.isArray(tasks)) throw new Error('저장 형식 오류');
    const ids = new Set();
    for (const t of tasks) {
      if (!t || typeof t.id !== 'string' || !t.id || ids.has(t.id) || typeof t.done !== 'boolean' || normalizeTitle(t.title) !== t.title) throw new Error('저장 형식 오류');
      ids.add(t.id);
    }
    return tasks.map(t => ({id:t.id, title:t.title, done:t.done}));
  }
  const api = { normalizeTitle, add, toggle, remove, decode };
  if (typeof module !== 'undefined' && module.exports) module.exports = api;
  else root.TodoCore = api;
})(typeof globalThis !== 'undefined' ? globalThis : this);

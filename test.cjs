'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');
const core = require('./core.js');
test('추가 및 원본 배열 보존', () => {
 const original = []; const tasks = core.add(original, '  테스트  ', '1');
 assert.deepEqual(tasks, [{id:'1',title:'테스트',done:false}]); assert.equal(original.length, 0);
});
test('공백, 121자, 중복 ID 입력 거부', () => {
 assert.throws(() => core.add([], '   ', '1'));
 assert.throws(() => core.add([], '가'.repeat(121), '1'));
 assert.equal(core.add([], '가'.repeat(120), '1')[0].title.length,120);
 assert.throws(() => core.add(core.add([], '첫째', '1'), '둘째', '1'));
});
test('완료 전환, 취소 및 삭제', () => {
 const tasks = core.add([], '테스트', '1');
 assert.equal(core.toggle(tasks,'1')[0].done,true);
 assert.equal(core.toggle(core.toggle(tasks,'1'),'1')[0].done,false);
 assert.deepEqual(core.remove(tasks,'1'),[]);
});
test('저장 기록 복원 및 잘못된 기록 거부', () => {
 const tasks = core.add([], '기록', '1');
 assert.deepEqual(core.decode(JSON.stringify(tasks)),tasks);
 assert.deepEqual(core.decode(null),[]);
 for (const bad of ['{', '{}', '[null]', '[{"id":"1","title":"a","done":"false"}]']) assert.throws(() => core.decode(bad));
});
function harness(initial = null, failWrite = false) {
 class Element {
  constructor() {this.children=[];this.events={};this.value='';this.textContent='';}
  append(...children){this.children.push(...children);}
  replaceChildren(){this.children=[];}
  addEventListener(name, handler){this.events[name]=handler;}
  setAttribute(){} focus(){}
 }
 const elements = Object.fromEntries(['#tasks','#task','#status','#summary','#empty','#add-form'].map(id=>[id,new Element()]));
 let saved = initial;
 const context = {window:{TodoCore:core},document:{querySelector:id=>elements[id],createElement:()=>new Element()},localStorage:{getItem:()=>saved,setItem:(_,value)=>{if(failWrite)throw Error('blocked');saved=value;}}};
 vm.runInNewContext(fs.readFileSync(path.join(__dirname,'app.js'),'utf8'),context);
 return {elements, saved:()=>saved, submit(title){elements['#task'].value=title;elements['#add-form'].events.submit({preventDefault(){}});}};
}
test('화면 연결: 추가 → 완료 → 저장 복원 → 삭제', () => {
 const h=harness(); h.submit('<img src=x onerror=alert(1)>');
 assert.equal(h.elements['#tasks'].children.length,1);
 const row=h.elements['#tasks'].children[0];
 assert.equal(row.children[0].children[1].textContent,'<img src=x onerror=alert(1)>');
 row.children[0].children[0].events.change();
 assert.equal(JSON.parse(h.saved())[0].done,true);
 const restored=harness(h.saved());
 assert.equal(restored.elements['#tasks'].children[0].children[0].children[0].checked,true);
 restored.elements['#tasks'].children[0].children[1].events.click();
 assert.equal(restored.saved(),'[]'); assert.equal(restored.elements['#empty'].hidden,false);
});
test('저장 실패 메시지 및 손상된 기존 기록 보존', () => {
 const blocked=harness(null,true);blocked.submit('새 기록');
 assert.match(blocked.elements['#status'].textContent,/저장할 수 없습니다/);
 const corrupt=harness('{');corrupt.submit('새 기록');assert.equal(corrupt.saved(),'{');
 assert.match(corrupt.elements['#status'].textContent,/메모리에만/);
});

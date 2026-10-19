# Laporan Perubahan Frontend — `FE-RWI-266`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-266` |
| Judul | Hook lifecycle realtime pemilihan bed |
| Slice | Notifikasi realtime konflik pemilihan bed — lifecycle selection |
| Roadmap | [`frontend-roadmap-realtime-bed-selection.md`](../../../roadmap/frontend-roadmap-realtime-bed-selection.md), kartu `FE-RWI-266` |
| Trace | `FR-RI-208`, `FR-RI-210`, `FR-RI-211`; `RWI-AC-472`, `474`–`477`; `VAL-RTB-06`–`09`; `UAT-910`, `911`, `913`–`915` |
| Contract version | `0.16.0`, approved melalui `RWI-DEC-337` dan delivery lock `RTB-DEL-20261017-01` |
| Wewenang UI | `NOT APPLICABLE` — hook tidak merender UI; callback tampilan dimiliki consumer |
| Dependency | `FE-RWI-265` 🟡; client dan kontrak method/event telah tersedia di source |
| Klasifikasi | `MEDIUM` — skor 6: lifecycle async, filter unit, actor suppression, dedup berbatas, reconnect/refetch, dan cleanup; tanpa route/Redux/package baru |
| Task mode | `CROSS-REPO`: source frontend; laporan, roadmap, dan traceability pada blueprint backend |
| Target tulis | Hook `use-inpatient-bed-realtime.js`, utility realtime minimum, test task; laporan dan tautan bukti |
| Model | Codex |
| Commit frontend saat dikerjakan | Snapshot awal `0accf387138a3fcc1ff40fbb5bdeb960076b4d59`; source task kemudian tercakup pada `685844a34bbeac58b2ba160348619ba165c11b61` oleh perubahan Git eksternal |
| Commit backend yang dijadikan rujukan | `457842f0f0a9f4da3ab7e3ac0ec76a60204020e6` (`MHamzah`) |
| Tanggal | 17 Oktober 2026 |
| Status | 🟡 **SEBAGIAN** — source dan test fokus tersedia; runtime lifecycle dan `D-FE` global belum terbukti penuh |

---

## 1. Keadaan yang ditemukan di awal

Sesudah FE-RWI-265, client dapat menerima event dan menyinkronkan unit, tetapi consumer masih akan
menanggung sendiri normalisasi payload, penyaringan unit, deduplikasi, pembandingan actor,
reconnect refresh, dan cleanup. Tanpa satu boundary hook, beberapa layar berisiko mendaftarkan
handler ganda atau memakai closure callback yang sudah stale.

## 2. Proses bisnis dari sisi pengguna

1. Layar mengaktifkan hook dengan identity user dan unit yang sedang tampil.
2. Hook menormalisasi/dedup GUID unit lalu menyinkronkannya ke client bersama.
3. Event invalid, event unit lain, atau `eventId` yang sudah pernah diterima diabaikan.
4. Event actor sendiri tidak menghasilkan alert, tetapi snapshot HTTP tetap dimuat ulang.
5. Event actor lain diteruskan satu kali ke callback alert, baru kemudian callback refresh dipanggil.
6. Reconnect hanya memicu refetch sesudah group di-join ulang; event lama tidak diputar ulang.
7. Unmount meninggalkan seluruh unit dan menghentikan connection milik hook.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/lib/realtime/queue-realtime-client.js`
- hook realtime queue/IGD existing sebagai referensi lifecycle
- `src/lib/state/slice/auth/login-slice.jsx`
- `src/lib/state/slice/auth/permission-slice.jsx`
- kontrak payload, validasi, acceptance matrix, dan frontend architecture `0.16.0`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-realtime.js` | Boundary connection/unit/callback; filter unit, dedup, suppress actor, refresh event/reconnect, cleanup leave/stop |
| `src/lib/realtime/inpatient-bed-realtime.js` | Utility murni untuk GUID, payload camel/Pascal case, distinct unit, tracker `eventId` kapasitas 200, dan pesan event |
| `tests/unit/inpatient-bed-realtime.test.mjs` | Test normalisasi, payload invalid, distinct GUID, dedup serta eviction berbatas, actor/refresh/reconnect/cleanup source contract |

### 3.3 Kepatuhan arsitektur frontend

Hook ditempatkan pada folder domain inpatient management dan memakai client realtime existing.
Utility murni dipisahkan ke `src/lib/realtime` agar dapat diuji tanpa render React. Callback disimpan
dalam ref sehingga pergantian `state.refresh` tidak membuat connection baru; pergantian user
membuat lifecycle baru. Tidak ada Redux slice, service HTTP, framework test, atau package baru.

## 4. State yang ditangani di layar

Hook tidak merender state sendiri.

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak ada indikator tambahan; loading tetap dimiliki `useFloorPlan` |
| Kosong | Unit kosong menonaktifkan hook dan tidak membuka subscription |
| Gagal | Callback refresh dapat mengaktifkan error existing; payload event tidak digunakan untuk menebak status bed |
| Tanpa hak akses | Join gagal secara fail-closed; hook tidak menampilkan payload atau alert bed |

## 5. Endpoint yang dikonsumsi

#### SignalR hub — `NOT APPLICABLE` untuk `[Tags(...)]`

| Transport | Path / method | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| SignalR | `/hubs/queues` · join/leave unit | Menjaga subscription unit aktif sesuai lifecycle hook | Authenticated dan lolos permission/scope backend |
| SignalR event | `InpatientBedSelectionChanged` | Menjalankan filter, dedup, actor suppression, alert callback, dan refresh callback | Hanya audience unit berwenang |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Lint terarah file task | Exit `0`, tanpa error | `PASS` | ESLint hook, utility, client, consumer, dan test |
| `npm run lint:errors` | Exit `0`, tanpa diagnostic | `PASS` | Pemindaian repository penuh |
| Test fokus realtime | 6/6 lulus | `PASS` | Payload camel/Pascal, unit normalization, bounded dedup, actor suppression contract, refresh/reconnect, cleanup |
| `npm run test:unit` | 2.985 total; 2.954 lulus; 31 gagal existing di luar test realtime | `EXISTING / ENVIRONMENT ISSUE` | Test realtime tidak gagal |
| Build production | Tidak lulus karena blocker environment/existing: junction Turbopack, OOM webpack 4 GB, lalu CSS module global Billing/IGD pada retry 8 GB | `EXISTING / ENVIRONMENT ISSUE` | Tidak ada diagnostic hook/utility realtime |
| `git diff --check` file task | Exit `0` | `PASS` | Tidak ada whitespace error task |
| Dua sesi: unit salah, actor sendiri, unit berganti, reconnect, callback gagal, unmount | Belum dijalankan | `NOT RUN` | Memerlukan runtime/backend/data/auth dua user |

AUTOMATED TEST: `npm run test:unit` — **FAIL** (31 kegagalan existing; test fokus task 6/6 PASS).

Uji manual: `NOT FEASIBLE` — tidak ada fixture dua identity, service unit berbeda, dan runtime backend
baru untuk menjalankan matriks lifecycle.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `RWI-AC-472` payload alert dan actor sendiri disuppress | Terpenuhi di source/test fokus; runtime belum | Payload dinormalisasi; perbandingan `actorUserId`; test contract PASS; `UAT-910` belum |
| `RWI-AC-474` unit lama dilepas dan event luar unit diabaikan | Terpenuhi di source; runtime belum | Active unit set, sync leave/join, cleanup `[]`; `UAT-913` belum |
| `RWI-AC-475` reconnect rejoin lalu refetch tanpa replay | Terpenuhi di source; runtime belum | Status connected kedua memanggil `onRefresh({reason: "reconnected"})`; `UAT-911` belum |
| `RWI-AC-476` state tetap berasal dari HTTP | Terpenuhi di source; runtime belum | Event hanya callback alert/refetch; tidak ada mutasi bed dari payload |
| `RWI-AC-477` duplicate/reconnect tidak menumpuk alert | Terpenuhi di test fokus; runtime belum | Tracker kapasitas 200 menolak event sama; eviction diuji; `UAT-914` belum |
| Error callback dan unmount aman | Terpenuhi di source; runtime belum | Error alert tidak menahan refresh; cleanup leave lalu stop; `UAT-915` belum |
| Ketiga command `V-FE` lulus | Belum terpenuhi | Lint lulus; suite/build global memiliki blocker existing |

Lima dari lima acceptance criteria telah dipetakan ke source; dedup memiliki test perilaku murni,
tetapi limanya belum memiliki bukti runtime lengkap. Status tetap 🟡.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tracker sengaja berada di memori per mount; kontrak melarang outbox/history/replay client |
| Masalah yang diketahui | Runtime callback error/reconnect cepat belum diuji; suite/build global terhalang masalah existing |
| Dependency backend | `BE-RWI-250` 🟡 menyediakan auth group dan event; verifikasi authenticated allow/deny belum dijalankan |
| Perubahan sampingan | `NONE`; utility hanya memuat kontrak realtime bed dan tidak menjadi abstraksi domain umum |
| Interupsi | Satu build retry terinterupsi pesan baru lalu diulang; source masuk commit eksternal `685844a34`, bukan melalui tindakan Git agent |
| Status Git | Frontend saat laporan hanya memiliki dua perubahan unrelated; source hook/utility/test task ada pada `685844a34` |
| Langkah berikutnya | Jalankan `UAT-910`, `911`, `913`, `914`, dan `915` pada runtime dua sesi setelah backend baru dijalankan |

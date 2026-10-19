# Laporan Perubahan Frontend — `FE-RWI-265`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-265` |
| Judul | Extend queue realtime client untuk unit selection |
| Slice | Notifikasi realtime konflik pemilihan bed — client SignalR bersama |
| Roadmap | [`frontend-roadmap-realtime-bed-selection.md`](../../../roadmap/frontend-roadmap-realtime-bed-selection.md), kartu `FE-RWI-265` |
| Trace | `FR-RI-202`, `FR-RI-210`, `FR-RI-211`; `RWI-AC-474`–`477`; `VAL-RTB-07`–`09`; `UAT-911`, `UAT-913` |
| Contract version | `0.16.0`, approved melalui `RWI-DEC-337` dan delivery lock `RTB-DEL-20261017-01` |
| Wewenang UI | `NOT APPLICABLE` — task ini tidak mengubah tampilan; hanya memperluas client realtime existing |
| Dependency | Tidak ada task pendahulu frontend; kontrak hub disediakan `BE-RWI-250` 🟡 |
| Klasifikasi | `MEDIUM` — skor 5: satu shared client berisiko memengaruhi consumer queue/IGD existing, menambah lifecycle group dan reconnect, tanpa route/package/UI baru |
| Task mode | `CROSS-REPO`: source frontend; laporan, roadmap, dan traceability pada blueprint backend |
| Target tulis | `QuilvianSystemFrontendDev/src/lib/realtime/queue-realtime-client.js`; laporan dan tautan bukti task ini |
| Model | Codex |
| Commit frontend saat dikerjakan | Snapshot awal `0accf387138a3fcc1ff40fbb5bdeb960076b4d59`; source task kemudian tercakup pada `685844a34bbeac58b2ba160348619ba165c11b61` oleh perubahan Git eksternal |
| Commit backend yang dijadikan rujukan | `457842f0f0a9f4da3ab7e3ac0ec76a60204020e6` (`MHamzah`) |
| Tanggal | 17 Oktober 2026 |
| Status | 🟡 **SEBAGIAN** — source dan test fokus tersedia; runtime SignalR, suite global, dan build global belum memenuhi `D-FE` |

---

## 1. Keadaan yang ditemukan di awal

Client `createQueueRealtimeConnection` sudah menangani autentikasi, reconnect, event queue/IGD,
serta sinkronisasi beberapa jenis group. Client belum mengenal event
`InpatientBedSelectionChanged`, belum menyimpan daftar unit pemilihan bed, dan belum memiliki
operasi join/leave/sync ke method hub yang dikunci kontrak `0.16.0`.

Membuat koneksi kedua akan menggandakan auth dan lifecycle. Karena itu client bersama diperluas
tanpa mengubah signature atau perilaku callback consumer lama.

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka layar pemilihan bed; hook pada task berikutnya memberikan daftar unit yang
   sedang tampil kepada client.
2. Client memastikan satu koneksi ke `/hubs/queues`, lalu memanggil
   `JoinInpatientBedSelection(serviceUnitId)` untuk unit baru.
3. Ketika event unit itu datang, client meneruskannya ke callback khusus pemilihan bed.
4. Saat unit berganti, client meninggalkan unit lama dan bergabung ke unit baru.
5. Setelah reconnect, seluruh unit aktif di-join ulang sebelum status `connected` diumumkan.
6. Saat layar berhenti memakai client, daftar unit dibersihkan dan koneksi dihentikan. Bila join
   ditolak atau koneksi gagal, tidak ada fallback ke broadcast maupun mutasi state bed lokal.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/lib/realtime/queue-realtime-client.js`
- `src/lib/hooks/health-services/registration-management/doctor-queue/use-doctor-queue.js`
- `src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-assessment-list.jsx`
- `Hubs/QueueHub.cs`
- `Program.cs`
- kontrak API, validasi, frontend architecture, dan roadmap realtime bed versi `0.16.0`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/realtime/queue-realtime-client.js` | Menambah nama event, callback, state distinct unit IDs, join/leave/sync, rejoin sesudah reconnect, reset saat stop, dan getter diagnostik tanpa mengubah consumer existing |
| `tests/unit/inpatient-bed-realtime.test.mjs` | Menambah bukti kontrak method/event/rejoin dengan runner `node:test` existing |

### 3.3 Kepatuhan arsitektur frontend

Client SignalR existing di-`EXTEND`; tidak ada client paralel, Axios instance, package, route, Redux,
atau base component baru. Auth cookie/token, retry delays, logging, dan lifecycle stop tetap memakai
pola `queue-realtime-client.js`. Logger tidak membaca atau mencetak payload event, sehingga nama
pasien/petugas tidak ditambahkan ke log frontend.

## 4. State yang ditangani di layar

Task ini tidak merender layar secara langsung.

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `NOT APPLICABLE` pada client; layar tetap memakai loading denah existing |
| Kosong | Daftar unit kosong tidak membuka koneksi baru dan tidak menampilkan alert |
| Gagal | Error diteruskan ke callback lifecycle; tidak ada status bed yang ditebak dari event |
| Tanpa hak akses | Hub menolak join; client tidak memasukkan user ke group dan tidak memiliki fallback broad audience |

## 5. Endpoint yang dikonsumsi

#### SignalR hub — `NOT APPLICABLE` untuk `[Tags(...)]`

Hub bukan controller Swagger, sehingga tidak memiliki atribut `[Tags]`.

| Transport | Path / method | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| SignalR | `/hubs/queues` · `JoinInpatientBedSelection(Guid)` | Bergabung ke audience unit pemilihan bed | Authenticated; `InpatientBedOccupancy:Read`, `InpatientEpisode:Read`, dan scope unit |
| SignalR | `/hubs/queues` · `LeaveInpatientBedSelection(Guid)` | Meninggalkan audience unit lama | Authenticated connection |
| SignalR event | `InpatientBedSelectionChanged` | Meneruskan perubahan bed committed ke hook | Hanya group unit yang berhasil diikuti |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Lint terarah file task | Exit `0`, tanpa error | `PASS` | ESLint pada client, utility, hook, component, dan test realtime |
| `npm run lint:errors` | Exit `0`, tanpa diagnostic | `PASS` | Command repository selesai setelah pemindaian penuh |
| Test fokus realtime | 6 test, 6 lulus, 0 gagal | `PASS` | `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-bed-realtime.test.mjs` |
| `npm run test:unit` | 2.985 test; 2.954 lulus, 31 gagal pada test existing lintas modul | `EXISTING / ENVIRONMENT ISSUE` | Tidak ada kegagalan pada `inpatient-bed-realtime.test.mjs` |
| `npm run build` | Turbopack isolasi tertahan junction; retry webpack 4 GB OOM; retry 8 GB mencapai compile lalu gagal pada selector CSS module existing Billing/IGD | `EXISTING / ENVIRONMENT ISSUE` | Tidak ada diagnostic pada file realtime bed |
| `git diff --check` file task | Exit `0`; hanya peringatan normalisasi LF/CRLF pada file existing | `PASS` | Lima file source/test task diperiksa |
| Reconnect, unit change, stop pada dua sesi authenticated | Belum dijalankan karena server aktif memakai runtime lama dan fixture dua user/unit tidak tersedia | `NOT RUN` | `UAT-911`, `UAT-913` masih memerlukan runtime |

AUTOMATED TEST: `npm run test:unit` — **FAIL** (31 kegagalan existing di luar test task; test fokus 6/6 PASS).

Uji manual: `NOT FEASIBLE` — belum tersedia dua sesi authenticated, identity permission berbeda,
unit uji, dan runtime backend build baru.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `RWI-AC-474` leave unit lama dan cleanup lifecycle | Terpenuhi di source; runtime belum | `syncInpatientBedSelectionUnits`, leave helper, reset `stop`; `UAT-913` belum dijalankan |
| `RWI-AC-475` reconnect rejoin dan snapshot baru | Terpenuhi di source; runtime belum | Unit aktif di-join ulang pada `onreconnected`; `UAT-911` belum dijalankan |
| `RWI-AC-476` HTTP tetap authoritative ketika realtime putus | Terpenuhi secara arsitektur; runtime belum | Client hanya meneruskan payload, tidak memutasi status bed; race runtime belum dijalankan |
| `RWI-AC-477` reconnect/delivery tidak menumpuk event | Terpenuhi sebagian | Satu handler event per connection dan rejoin distinct IDs; bukti reconnect berulang belum tersedia |
| Ketiga command `V-FE` lulus | Belum terpenuhi | Lint lulus; suite global dan build global tertahan masalah existing |
| Laporan, roadmap, traceability | Terpenuhi | Laporan ini beserta tautan status 🟡 |

Empat dari empat acceptance criteria telah dipetakan ke source, tetapi empatnya belum memiliki bukti
runtime lengkap. `D-FE` belum terpenuhi, sehingga status tetap 🟡.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Production gate `RWI-OQ-151` tetap terbuka; nama lengkap tidak boleh diaktifkan di produksi sebelum sign-off privacy/security |
| Masalah yang diketahui | Suite global gagal 31 test existing; build global gagal pada CSS module Billing/IGD di luar scope |
| Dependency backend | `BE-RWI-250` source/build tersedia dengan status 🟡; allow/deny runtime belum terbukti |
| Perubahan sampingan | `NONE`; tidak ada package, route, menu, permission, atau CSS yang ditambahkan task ini |
| Interupsi | Pesan `continue` menginterupsi satu build 8 GB dan build kemudian diulang; commit/push `685844a34` muncul dari luar sesi, agent tidak menjalankan Git write |
| Status Git | Frontend saat laporan: hanya perubahan unrelated pada `use-inpatient-admission-flow.jsx` dan `inpatient-newborn-admission.test.mjs`; source task sudah berada pada commit `685844a34` |
| Langkah berikutnya | Jalankan `UAT-911` dan `UAT-913` memakai dua sesi dan runtime backend baru, lalu ulangi build setelah blocker CSS existing diperbaiki owner terkait |

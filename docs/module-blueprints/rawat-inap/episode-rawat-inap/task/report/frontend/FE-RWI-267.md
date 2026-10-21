# Laporan Perubahan Frontend — `FE-RWI-267`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-267` |
| Judul | Integrasi alert, refresh, dan stale clear pada FloorPlanSelection |
| Slice | Notifikasi realtime konflik pemilihan bed — pengalaman pengguna akhir |
| Roadmap | [`frontend-roadmap-realtime-bed-selection.md`](../../../roadmap/frontend-roadmap-realtime-bed-selection.md), kartu `FE-RWI-267` |
| Trace | `FR-RI-208`, `FR-RI-209`; `RWI-AC-469`, `471`, `472`, `476`; `VAL-RTB-06`, `09`, `11`; `UAT-901`–`903`, `905`, `910`, `912`, `915`, `916` |
| Contract version | `0.16.0`, approved melalui `RWI-DEC-337` dan delivery lock `RTB-DEL-20261017-01` |
| Wewenang UI | Reuse komponen existing untuk alert nonblocking; tidak mengubah layout/geometri/style denah dan tidak membuat base component baru |
| Dependency | `FE-RWI-266` 🟡 dan `BE-RWI-251` 🟡; source keduanya tersedia, bukti runtime belum lengkap |
| Klasifikasi | `MEDIUM` — skor 5: wiring satu shared selection component, tiga message contract, refresh/stale safety dan a11y; tanpa CSS/route/package baru |
| Task mode | `CROSS-REPO`: source frontend; laporan, roadmap, dan traceability pada blueprint backend |
| Target tulis | Integrasi minimum `floor-plan-selection.jsx`, utility/hook realtime task, test; laporan dan tautan bukti |
| Model | Codex |
| Commit frontend saat dikerjakan | Snapshot awal `0accf387138a3fcc1ff40fbb5bdeb960076b4d59`; source task kemudian tercakup pada `685844a34bbeac58b2ba160348619ba165c11b61` oleh perubahan Git eksternal |
| Commit backend yang dijadikan rujukan | `457842f0f0a9f4da3ab7e3ac0ec76a60204020e6` (`MHamzah`) |
| Tanggal | 17 Oktober 2026 |
| Status | 🟡 **SEBAGIAN** — implementasi UI dan test fokus tersedia; runtime dua sesi serta `D-FE` global belum terbukti |

---

## 1. Keadaan yang ditemukan di awal

`FloorPlanSelection` sudah memiliki `state.refresh` dan effect yang memanggil `onClearSelection`
ketika bed terpilih hilang atau tidak lagi eligible. Layar belum berlangganan perubahan bed,
belum mengetahui actor session, dan belum menampilkan notifikasi nonblocking ketika user lain
memesan, menempatkan, atau mentransfer pasien ke bed yang sedang diamati.

File selection sudah memiliki perubahan pengguna lain (`autoCloseOnSelect`, selected room/bed,
skeleton, dan guard kamar selectable). Perubahan realtime digabung secara lokal tanpa mengganti
atau menghapus perilaku tersebut.

## 2. Proses bisnis dari sisi pengguna

1. User 1 membuka pemilihan bed dan dapat tetap scroll walaupun popup kamar tertutup.
2. Layar mengumpulkan distinct `serviceUnitId` dari data floor plan dan membuka subscription.
3. User 2 menyelesaikan reservasi, penempatan awal, atau transfer pada unit yang sama.
4. User 1 segera melihat toast, misalnya
   **“Bed 202 telah didaftarkan atas nama pasien Sintia oleh user Rendi.”**
5. Setelah alert dijadwalkan, layar memanggil `state.refresh`; event tidak pernah langsung mengubah
   status bed.
6. Jika bed yang sedang dipilih tidak lagi eligible atau hilang, effect existing memanggil
   `onClearSelection` sehingga submit stale tidak diteruskan.
7. Jika refresh gagal, toast tetap tampil dan `InformationAlert` existing menampilkan error.
8. Actor yang melakukan mutasi tidak melihat alert sendiri, tetapi denahnya tetap direfresh.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `floor-plan-selection.jsx`, `floor-plan-view.jsx`, `floor-plan-room-popup.jsx`
- `use-floor-plan.js`
- `ToastStack`, `InformationAlert`, dan pemakai `ToastStack` terdekat
- `selectUserInfo` / permission owner selector
- service/controller `GET .../bed-occupancies/floor-plan`
- frontend architecture 20, contract API 17, flow 21, acceptance matrix 24

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-selection.jsx` | Membaca identity/unit, memakai hook realtime, membentuk maksimal lima toast, menyediakan dismiss dan live-region, lalu reuse `state.refresh` serta stale effect existing |
| `src/lib/realtime/inpatient-bed-realtime.js` | Membentuk tiga pesan sesuai `eventType`, fallback `bedName` → `bedCode`, pasien/actor defensif |
| `tests/unit/inpatient-bed-realtime.test.mjs` | Membuktikan tiga kalimat pesan, reuse `ToastStack`, live region, dan stale-selection guard |

### 3.3 Kepatuhan arsitektur frontend

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Notifikasi | `REUSE` `ToastStack` | Dismissible, nonblocking, base component existing |
| Error refresh | `REUSE` `InformationAlert` | Sudah dimiliki alur `useFloorPlan`/selection |
| Sumber kebenaran | `REUSE` `state.refresh` | Snapshot HTTP dan eligibility server tetap authoritative |
| Stale selection | `REUSE` effect existing | Bed hilang/tidak eligible memanggil `onClearSelection` |
| CSS/base component | `NOT CREATED` | Tidak ada nilai visual literal atau stylesheet task |

UI GATE: `REUSE` seluruh elemen; tidak ada keputusan `NEW`/perubahan default base component yang
memerlukan approval tambahan.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `FloorPlanSkeleton` existing; subscription belum aktif sampai data unit tersedia |
| Kosong | Tidak ada subscription bila respons belum memiliki unit/bed; layar mempertahankan perilaku denah existing |
| Gagal | Initial error menampilkan `InformationAlert` dan **Muat Ulang Denah**; error refresh mempertahankan data/toast dan menampilkan alert |
| Tanpa hak akses | HTTP 403 memakai error denah existing; join SignalR gagal tertutup dan tidak menampilkan payload |
| Event actor lain | Toast warning top-end, dapat ditutup, `role=status`, `aria-live=polite`, maksimal lima item |
| Event actor sendiri | Tidak ada toast; refresh HTTP tetap berjalan |
| Pilihan menjadi stale | Pilihan dibersihkan setelah respons refresh tidak lagi eligible |

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/floor-plan?floorCode=...&purpose=...&episodeId=...` | Snapshot authoritative sesudah event/reconnect dan evaluasi stale selection | `InpatientBedOccupancy : Read` |

#### SignalR hub — `NOT APPLICABLE` untuk `[Tags(...)]`

| Transport | Path / event | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| SignalR | `/hubs/queues` · `InpatientBedSelectionChanged` | Trigger alert lalu refetch | Authenticated; kedua permission read dan scope unit pada join |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Keputusan base component | `ToastStack`, `InformationAlert`, denah existing seluruhnya direuse; tanpa CSS baru | `PASS` | Import dan JSX selection |
| Lint terarah | Exit `0` | `PASS` | Seluruh file source/test realtime |
| `npm run lint:errors` | Exit `0` | `PASS` | Repository penuh |
| Test fokus realtime | 6/6 lulus | `PASS` | Tiga pesan exact, a11y, stale guard, normalisasi/dedup/lifecycle contract |
| `npm run test:unit` | 2.985 total; 2.954 lulus; 31 gagal existing lintas CRM/HMD/Operasi/Finance/rawat-inap lama | `EXISTING / ENVIRONMENT ISSUE` | `inpatient-bed-realtime.test.mjs` seluruhnya lulus |
| Build production | Compile global gagal pada pure selector CSS module existing Billing/IGD setelah retry 8 GB | `EXISTING / ENVIRONMENT ISSUE` | Tidak ada diagnostic realtime/selection task |
| `git diff --check` file task | Exit `0` | `PASS` | Tidak ada whitespace error task |
| Dua browser reserve/place/transfer, same/cross-unit, actor, scroll/popup, refresh gagal, race, reconnect | Belum dijalankan | `NOT RUN` | Backend aktif saat pengerjaan menggunakan binary lama; fixture/kredensial tidak tersedia |

AUTOMATED TEST: `npm run test:unit` — **FAIL** (31 kegagalan existing; test fokus realtime 6/6 PASS).

Uji manual: `NOT FEASIBLE` — skenario final membutuhkan dua sesi authenticated, tiga mutasi bed,
unit yang sama/berbeda, failure injection, dan backend runtime build baru.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `RWI-AC-469` reservasi menghasilkan alert/refetch/stale clear | Terpenuhi di source; runtime belum | Pesan `BedReserved`, alert-before-refresh, stale effect; `UAT-901/912` belum |
| `RWI-AC-471` placement/transfer target dan race authoritative | Terpenuhi di source; runtime belum | Pesan `PatientPlaced`/`PatientTransferred`; event hanya refetch; `UAT-902/903/905` belum |
| `RWI-AC-472` isi alert, actor suppression, accessibility | Terpenuhi di source/test; runtime belum | Tiga pesan exact, actor compare pada hook, dismiss/live region; `UAT-910/916` belum |
| `RWI-AC-476` SignalR tidak menggantikan HTTP/409 guard | Terpenuhi di source; runtime belum | `state.refresh` tetap source of truth dan stale effect existing dipertahankan |
| Alert tetap ada bila refresh gagal | Terpenuhi di source; runtime belum | Toast state dibuat sebelum callback refresh; error `useFloorPlan` tidak menghapus toast |
| Popup-independent dan scroll nonblocking | Terpenuhi di source; visual runtime belum | Hook berada pada owner `FloorPlanSelection`; `ToastStack` di luar popup, tanpa modal/overlay baru |
| Ketiga command `V-FE` lulus | Belum terpenuhi | Lint lulus; suite/build global tertahan issue existing |

Empat dari empat acceptance criteria telah dipetakan ke source dan pesan/a11y mempunyai test fokus,
namun bukti runtime dua sesi belum ada. Roadmap sendiri membatasi FE-RWI-267 maksimal 🟡 tanpa
runtime dua sesi; status task tetap 🟡.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nama lengkap adalah data sensitif; gate produksi `RWI-OQ-151` masih terbuka dan log frontend tidak mencetak payload |
| Masalah yang diketahui | Tidak ada bukti visual/live SignalR; suite/build global memiliki kegagalan existing di luar file task |
| Dependency backend | `BE-RWI-251` 🟡: publish post-commit ada di source/build, tetapi reserve/place/transfer runtime belum diuji |
| Perubahan sampingan | `NONE`; perubahan pengguna pada selection (skeleton, selectable guard, auto-close, selected room/bed) dipertahankan dan ikut tercakup dalam commit eksternal |
| Interupsi | Build heap 8 GB pertama terinterupsi oleh pesan baru lalu diulang; Git berubah ke commit/origin `685844a34` dari luar sesi, tanpa tindakan commit/push agent |
| Status Git | Frontend saat laporan hanya memiliki dua file unrelated modified; source realtime task berada pada `685844a34` |
| Langkah berikutnya | Jalankan `UAT-901`–`903`, `905`, `910`, `912`, `915`, `916`; perbaiki blocker CSS global oleh owner terkait lalu ulang `npm run build` |

# Kesiapan `MVP-7` — Encounter-first (`EPIC IGD-11`) — 3 Oktober 2026

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` · revision **`8`** · status `draft` (irisan encounter-first `approved` lewat `IGD-DEC-157`) |
| Slice | `MVP-7` = `EPIC IGD-11`: `FR-IGD-069`…`085`, `AT-IGD-166`…`185`; task `BE-IGD-051`…`055`, `057`…`059`, `FE-IGD-035`, `036`, `038`, `039`, `040`. **Di luar slice:** `EPIC IGD-12` (`BE-IGD-056`, `FE-IGD-037`, ⛔ `IGD-OQ-102`/`103`) |
| **Putusan** | **`READY_WITH_CONDITIONS`** — siap masuk UAT dan rilis bertahap ke lingkungan berikutnya dengan tujuh syarat pada bagian 5. **Belum** siap ditandatangani untuk produksi: DoD butir 10 (UAT) belum terpenuhi. *Diperbarui 3 Oktober 2026 (sore): C6 dan C2 lunas — DoD butir 8 terpenuhi lewat `IGD-DEC-181`; sisa syarat C1, C3, C4, C5, C7*. *Diperbarui lagi (uji tahap 1): C5 lunas, DoD butir 2 terpenuhi; **C4 gagal** — layar loket IGD memanggil rute kiosk `POST /patient-encounters` sehingga petugas loket sungguhan ditolak `403` (perlu perbaikan sebelum UAT). Sisa syarat: C1, C3, C4, C7* |
| Skill | `verify-module-readiness` — hanya membaca source dan bukti; tidak mengerjakan perbaikan |
| Wewenang tulis | `MODULE BLUEPRINT MODE` untuk berkas ini, diberikan Rizki Gunawan 3 Oktober 2026 (*"lanjutkan langkah berikutnya sesuai rekomendasi anda"*) |
| Backend diperiksa | `NewQuilvianSystemBackend` `rizkiG` **`5af6ef3b`**, source tanpa perubahan working tree (`ahead 3` terhadap origin) |
| Frontend diperiksa | `QuilvianSystemFrontendDev` `RizkiV2` **`521b18a9a`**, working tree bersih (`ahead 2`); build agent 3 Oktober 10.14 identik dengan commit ini |
| Kontrak | API `0.11.0` §8, validation `0.8.0` §10, state `0.5.0` §8, integration `0.4.0` §5, permission/audit `0.5.0` §7 (`IGD-DEC-157`) + koreksi `IGD-DEC-178` (0j.2) dan `IGD-DEC-179` (0j.3). Berkas kontrak kini berversi `0.13.0`/`0.10.0`/`0.7.0`/`0.6.0`/`0.5.0` karena slice lain; bagian §8/§10/§8/§5/§7 tidak disentuh sesudah approval |
| Hash masukan | Kelima kontrak **identik** dengan kunci manifest 0j/0j.3 (`653d1518…`, `bc247e47…`, `20f0004c…`, `24234cb2…`, `0f0a544f…`; isi dinormalkan LF). Decision log `d860a10f…` (0j.4, 180 keputusan; nilai sesudah penyelarasan C6) |
| Keputusan relevan | `IGD-DEC-139`, `142`…`148`, `150`…`154`, `157`…`162`, `178`, `179`, `180`; keterbatasan diterima `IGD-DEC-158`, `IGD-DEC-180` |

---

## 1. Putusan dalam bahasa sederhana

Seluruh **13 task** `MVP-7` sudah selesai dan masing-masing punya laporan beserta bukti uji. Alur barunya berjalan di
lingkungan pengembangan:

- petugas loket hanya mendaftarkan pasien;
- perawat triage yang memulai kunjungan IGD (Mulai Triage atau Tangani Segera);
- pendaftaran ganda ditolak di pintu;
- pasien yang pergi sebelum ditriage bisa ditandai;
- kunjungan yang berakhir ikut menutup encounter-nya.

Yang **belum** ada adalah uji oleh pengguna sesungguhnya (UAT), persetujuan pemilik modul Registrasi atas perubahan di
modul mereka, pengujian dengan akun peran nyata (bukan SuperAdmin), dan rilis ke lingkungan selain dev. Karena itu
putusannya *siap dengan syarat*: layak dibawa ke UAT, tetapi belum layak dirilis ke pasien sungguhan.

*Contoh syarat yang paling berisiko bila dilewati:* bila penjaga pintu encounter (`BE-IGD-053`) dirilis ke sebuah
lingkungan **sebelum** rekonsiliasi encounter lama (`BE-IGD-052`) dijalankan di sana, pasien yang encounter lamanya
tertinggal terbuka akan **ditolak** saat didaftarkan ulang, padahal kunjungan lamanya sudah selesai.

---

## 2. Cakupan dan denominator

| Yang dihitung | Jumlah | Hasil |
| --- | ---: | --- |
| Functional requirement `FR-IGD-069`…`085` | 17 | 17 terpetakan ke task dan skenario uji (traceability R3.13.2) |
| Skenario uji `AT-IGD-166`…`185` | 20 | 20 terpetakan; seluruhnya dijalankan pemilik — 9 task berbukti mentah yang diperiksa agent, 4 task atas pernyataan pemilik (bagian 4) |
| Task `MVP-7` | 13 | 13 ✅ — backend 8, frontend 5; 13 laporan tracked ada |
| Butir DoD PRD §8.6 | 10 | 9 terpenuhi, 1 belum (UAT) — *sebelum C5: 8, 1, 1; sebelum C2: 7, 1, 2* |

---

## 3. Skor per dimensi

**Kemajuan pengerjaan (scaffold dan implementasi): 13 dari 13 task = 100%.**
**Kesiapan ujung-ke-ujung (berbobot): ±85%** sesudah uji tahap 1 (C5 naik, C4 menurunkan frontend; ±80% saat audit pertama) — rinciannya di bawah. Kedua angka itu sengaja dipisah: semua task
selesai tidak sama dengan siap dipakai.

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
| --- | ---: | ---: | --- | --- |
| Fondasi — keputusan, kontrak, traceability | 15% | 6/6 (100%) | Kontrak `approved` dan hash utuh (manifest 0j.4); 17/17 FR dan 20/20 AT terpetakan; gate requirement `S1`…`S6`, `S8` siap (`evidence/02-requirement-completeness-gate.md`) | — (empat ketidaksesuaian dokumen bagian 6 dibereskan C6) |
| Backend | 25% | 8/8 (100%) | `…/Services/EmergencyEpisodeRule.cs#IsEncounterEnded,FindOpenEpisodeAsync@5af6ef3b`; `…/Controllers/EmergencyVisitController.cs#triage-queue,start-triage,no-show,arrival-time@5af6ef3b`; `…/Controllers/EmergencyEncounterReconciliationController.cs#preview,runs,reverse@5af6ef3b`; `…/RegistrationManagement/Controllers/PatientEncounterController.cs#GuardEncounterRegistrationAsync(:619),Emergency status(:1001),LockPatientEpisodeAsync(:1152)@5af6ef3b`; tiga migration (`20260923021224`, `20260923061124`, `20261001023520`) dengan `Down()` berpenjaga | — |
| Frontend | 20% | 4/5 (80%) — cacat loket `C4-01` | Build agent `521b18a9a` 465/465 halaman 0 warning; `eslint` 0 error; unit test IGD 91/91; kemunduran loket `4520abe64` dipulihkan dan diuji ulang pada hasil build (`FE-IGD-034`, `038`) | — |
| Integrasi dan runtime | 20% | 3/5 (60%) | Dev: tiga migration diterapkan; rekonsiliasi K1 dijalankan (`BE-IGD-052` R4); urutan rilis R3.13.5 dipatuhi di dev (`051` 22 Sep → `052` 23 Sep → `053` 1 Okt) ; persetujuan Registrasi `IGD-DEC-181` | Peran nyata belum diuji (C4); lingkungan lain belum dirilis (C3) |
| Verifikasi | 20% | 5/6 (83%) | DoD 1, 2, 3, 4, 6 terpenuhi (DoD 2 lewat C5 tahap 1); seluruh uji layar terakhir pada hasil build | DoD 2 sebagian (C5); DoD 10 UAT belum (C1) |

Skor berbobot sesudah uji tahap 1: 0,15×100 + 0,25×100 + 0,20×80 + 0,20×60 + 0,20×83 ≈ **85%**. Sesudah C2: 0,15×100 + 0,25×100 + 0,20×100 + 0,20×60 + 0,20×75 ≈ 87% (audit pertama: 0,15×83 + 0,25×100 + 0,20×100 + 0,20×40 + 0,20×75 ≈ 80%).

---

## 4. Definition of Done `EPIC IGD-11` (PRD §8.6)

| No | Butir | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | `AT-IGD-166`…`185` dijalankan pemilik, hasil tercatat per skenario | **Terpenuhi** | Laporan task 13/13. **Bukti mentah diperiksa agent:** `BE-IGD-053` (S1–S12), `055` (delta D1–D6), `057` (S1–S10), `058` (S1–S9), `059` (S1–S8), `FE-IGD-036`, `038`, `039`, `040`. **Pernyataan pemilik tanpa lampiran, dicatat apa adanya:** `BE-IGD-051` (S1–S9), `052` (10 skenario), `054` (11 skenario; S2 berlampir), `055` asli (S1–S15; S13 berlampir) |
| 2 | Uji paralel `AT-IGD-169` dan `AT-IGD-174`: tepat satu episode / satu kunjungan, hitungan baris dicatat | **Terpenuhi — 3 Oktober 2026** (`AT-IGD-174`: tiga putaran serentak berselisih 1 ms, `jumlah_kunjungan = 1`, status akhir 4 — [laporan uji tahap 1](../testing/2026-10-03-laporan-uji-tahap-1-mvp-7.md)). *Sebelumnya: sebagian* | `AT-IGD-169`: `BE-IGD-053` S4 `[200, 409]`, satu encounter di basis data (bukti mentah). `AT-IGD-174`: `BE-IGD-055` S6 lulus **menurut pemilik** — hitungan baris tidak dilampirkan. Penguat: `BE-IGD-057` S8 tiga putaran `Promise.all`, tiap putaran tepat satu berhasil; `036-U8` dua tab → kunjungan yang sama |
| 3 | Tiga migration punya `Down()` berpenjaga, diuji di basis data terpisah | **Terpenuhi** | `20260923021224` dan `20261001023520`: empat tahap, dijalankan agent; `20260923061124`: atas penilaian pemilik |
| 4 | Snapshot EF hanya bertambah blok tabel slice | **Terpenuhi** | `BE-IGD-052` +223/−0 (agent); `BE-IGD-053` +211/−92, baris terhapus dijelaskan sebagai susunan ulang (laporan §5); `BE-IGD-055` nol blok modul lain hilang sesudah merge |
| 5 | Nol kolom baru pada `RegPatientEncounter`; nol baris baru `Program.cs` | **Terpenuhi** | `git log 0d13f3a8..5af6ef3b -- …/Models/RegPatientEncounter.cs` kosong; perubahan `Program.cs` pada rentang itu milik modul lain — baris IGD satu-satunya hanya bergeser indentasi |
| 6 | Kueri invarian "kunjungan berakhir, encounter terbuka" = 0 sesudah rilis | **Terpenuhi di dev** | 0 baris sejak 22 September dan atas seluruh data, dua kali 3 Oktober (pernyataan pemilik; keluaran kueri tidak disimpan). Lingkungan lain: sesudah rilis di sana (C3) |
| 7 | Kontrak `approved`, hash dihitung ulang | **Terpenuhi** | `IGD-DEC-157`; hash utuh (manifest 0j.4) |
| 8 | Perubahan berkas Registrasi disetujui pemilik Registrasi secara tertulis | **Terpenuhi — atas keputusan pemilik** | `IGD-DEC-181` (3 Oktober 2026): pemegang dan pengembang modul Registrasi menyetujui `IGD-REQ-002` butir 1–5 secara lisan; pemilik IGD memutuskan konfirmasi tertulis dan nama tidak disyaratkan untuk tim internal. *Sebelumnya: belum — pemilik Registrasi belum dipetakan* |
| 9 | Butir wajib tinjau klinis (`IGD-DEC-150`) tercatat belum ditinjau | **Terpenuhi** | Traceability R3.13.5 baris *Tinjauan klinis*: `IGD-DEC-142`, `147`, `152`, `160`, `IGD-ASM-001`/`002` |
| 10 | UAT oleh tim UAT terpisah | **Belum** | Seluruh laporan menulis *tanpa UAT* (C1) |

---

## 5. Syarat (`READY_WITH_CONDITIONS`)

| Kode | Syarat | Pemilik | Mitigasi / cara memenuhi | Risiko bila dilewati |
| --- | --- | --- | --- | --- |
| **C1** | UAT oleh tim terpisah atas `AT-IGD-166`…`185` dan cerita UAT PRD §8.4 (Pak Rayyan, Bu Sari) | Rizki → tim UAT | Pakai cerita berhasil/gagal PRD §8.4 sebagai naskah; jalankan dengan akun peran nyata (C4) | Alur pendaftaran–triage berubah total bagi petugas; kebiasaan lama (loket mengisi waktu tiba) dapat menimbulkan salah pakai |
| **C2** | ✅ **Lunas 3 Oktober 2026 — `IGD-DEC-181`.** *Sebelumnya:* 🟡 **Disetujui lisan, menunggu konfirmasi tertulis** — Rizki melaporkan 3 Oktober 2026 bahwa pemegang modul Registrasi dan pengembang Registrasi menyetujui secara lisan ([`IGD-REQ-002`](../approval-requests/2026-10-03-permintaan-persetujuan-pemilik-registrasi.md)); nama dan konfirmasi tertulis belum ada, sehingga DoD butir 8 belum terpenuhi. Persetujuan tertulis pemilik Registrasi atas titik sentuh: penjaga `POST /patient-encounters` Emergency, penolakan `PATCH …/status`, pembatasan `…/cancel` | Rizki — memetakan pemilik Registrasi | Ajukan ringkasan API §8.1 nomor 1–4 dan integration §5; sampai disetujui, perubahan Registrasi tidak dirilis ke produksi | Tim Registrasi mengubah `PatientEncounterController` tanpa tahu penjaga IGD, sehingga pendaftaran ganda kembali terbuka |
| **C3** | Rilis per lingkungan mengikuti R3.13.5: `051` → `052` + **jalankan rekonsiliasi K1** (pratinjau → `expectedCount` → eksekusi) → `053` bersama `FE-IGD-038` → `057`, `059` → `FE-IGD-036`; lalu kueri invarian | Rizki | Urutan tidak ditegakkan perkakas — jadikan daftar periksa rilis; catat angka pratinjau dan hasil invarian per lingkungan | Pasien dengan encounter lama tertinggal terbuka (K1) ditolak saat datang lagi |
| **C4** | ❌ **Gagal pada uji tahap 1 (3 Oktober 2026): 9 dari 10 terbukti.** Perawat, admin data, dan akun tanpa IGD berperilaku benar; **`C4-01` gagal** — layar loket IGD memanggil `POST /patient-encounters` (rute kiosk, `KioskRead`, sejak `27c48484`), sehingga petugas loket sungguhan ditolak `403`; rute petugas `POST /patient-encounters/admin` bekerja. **Perbaikan dikerjakan 3 Oktober 2026** sebagai pengerjaan ulang `FE-IGD-036` (`IGD-DEC-182`, catatan API §8.1) — menunggu build dan uji ulang `C4-01` dengan akun loket. *Rumusan awal:* Pemetaan izin pada **peran nyata**, lalu satu putaran uji dengan akun non-SuperAdmin: `EmergencyVisit : NoShow` untuk perawat triage; `EmergencyEncounterReconciliation : Read/Process/Reverse` hanya untuk admin data (permission §7.1); `EmergencyVisit : Create` dipakai bersama loket dan Mulai Triage (`IGD-DEC-158`) | Rizki / admin peran | Katalog izin terbentuk otomatis dari atribut `[AccessController]`/`[AccessAction]`; pemberian ke peran lewat layar Akses Role. Seluruh uji sejauh ini memakai SuperAdmin; `FE-IGD-035` U10 (tanpa hak) hanya disimulasikan | Perawat tidak bisa menandai pasien pergi, atau peran klinis bisa menjalankan rekonsiliasi |
| **C5** | ✅ **Lunas 3 Oktober 2026** — tiga putaran serentak, hitungan baris tersimpan. Bukti hitungan baris `AT-IGD-174` | Rizki | Ulangi `BE-IGD-055` S6 dengan kueri hitungan laporan §6 (`SELECT COUNT(*) … FROM "EmgVisit" WHERE "EncounterId" = …`), simpan keluarannya | Rendah — perilaku serentak sudah dikuatkan `057-S8` dan `036-U8` |
| **C6** | ✅ **Selesai 3 Oktober 2026** — rapikan empat ketidaksesuaian dokumen pada bagian 6 | Agent — `manage-module-blueprint` / `plan-module-delivery` | Hanya tanda status dan baris status; tanpa perubahan kontrak | Pembaca roadmap salah membaca keadaan `MVP-7` |
| **C7** | Push `RizkiV2` (`ahead 2`, memuat pemulihan loket) dan `rizkiG` (`ahead 3`); commit dokumen blueprint 3 Oktober | Rizki | Push kedua commit frontend **bersamaan** — `4520abe64` tanpa `521b18a9a` membawa kemunduran loket | Lingkungan bersama menerima kemunduran loket, atau tidak menerima perbaikan |

---

## 6. Ketidaksesuaian dokumen dan bukti `STALE`

| Temuan | Letak | Tindakan |
| --- | --- | --- |
| Node ringkasan `R313` masih *"🟡 … BE-IGD-051 siap"*, padahal 8 task backend R3.13 ✅ | `roadmap/backend-roadmap.md` baris 129 (grafik ringkasan) | Ganti tanda menjadi ✅ dan kelas `selesai` (C6) |
| Node prasyarat `BE-IGD-054` tanpa tanda ✅ | `roadmap/frontend-roadmap.md` baris 1274 (grafik R3.12.1) | Tambah tanda ✅ (C6) |
| `IGD-OQ-108` masih tertulis `open` | `roadmap/requirement-traceability.md` R3.13.3 | Rujuk `IGD-DEC-180` (C6) |
| `IGD-OQ-093` masih `superseded` sebagian walau realisasinya terbukti 1 Oktober | `00-interview-decisions.md` | Catat penutupan realisasi `BE-IGD-053` (C6) |
| Capability map suplemen 3.2 dicatat pada `0d13f3a8`/`c941012ac` | `01-existing-capability-map.md` | `STALE` terhadap `5af6ef3b`/`521b18a9a`; tidak menahan `MVP-7` karena seluruh perilaku slice sudah dibuktikan langsung pada source |
| Bukti uji layar `FE-IGD-035` (30 Sep) dan `FE-IGD-039` (1–2 Okt) mungkin diambil sebelum perombakan tampilan `8cc155e02` (2 Okt) yang memindahkan aksi baris ke menu *Aksi* | Laporan kedua task | **Risiko rendah:** syarat munculnya aksi tetap dari `availableActions` backend (`emergency-triage-patient-table.jsx` baris 151–203); jalur menu *Aksi → Mulai Triage* terbukti 3 Oktober. Aksi *Pergi sebelum ditriage* lewat menu baru belum dilalui — tercakup UAT (C1) |

---

## 7. Risiko yang diterima atau dicatat (bukan syarat)

| Risiko | Dasar | Catatan |
| --- | --- | --- |
| Pasien kembali saat kunjungan lamanya menunggu penutupan dan tertahan `403` kewenangan unit tidak dapat dimulai kunjungan barunya | `IGD-DEC-180` | Ditinjau ulang bila `BE-IGD-039` selesai |
| Izin `EmergencyVisit : Create` dipakai bersama loket dan Mulai Triage | `IGD-DEC-158` | Petugas loket secara teknis dapat memanggil Mulai Triage lewat API |
| Kewajiban konfirmasi waktu tiba hanya ditegakkan layar | `IGD-DEC-160` | Klien lain dapat menyimpan triage tanpa konfirmasi |
| Tombol *Sekarang* pada pemilih jam waktu tiba mengisi jam dari jam browser bila ditekan | Laporan `FE-IGD-036` §10.6 | Menunggu jawaban pemilik |
| `TrxQueue` lama yang tertaut encounter Emergency tidak dibersihkan | `IGD-UNK-06` | Data lama saja |
| Praktik lapangan pasien tanpa identitas belum diketahui | `IGD-UNK-10` | Memengaruhi `AT-IGD-184` saat UAT |
| Tinjauan klinis dan keperawatan belum ada pemegang peran | `IGD-DEC-150`, `IGD-OQ-099` | Dicatat, tidak menahan (DoD 9) |
| Kebersihan keamanan di luar slice | — | Tiga `DataProtectionKeys/key-*.xml` ter-track di backend; kredensial teks biasa di bukti uji lokal (`test-with-agy/`, di-gitignore) |

---

## 8. Langkah berikutnya

Satu task terdekat yang langsung dapat dikerjakan: **C6** — perbaikan empat tanda dan baris status dokumen, lalu
**C7** oleh pemilik (commit dan push). Sesudahnya **C4** (pemetaan izin peran nyata) menjadi prasyarat **C1** (UAT).
**C2** dan **C3** berjalan paralel dan wajib selesai sebelum rilis produksi.

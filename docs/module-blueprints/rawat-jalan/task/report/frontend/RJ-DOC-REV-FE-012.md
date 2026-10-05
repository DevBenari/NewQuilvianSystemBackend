# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-012` |
| Judul | Konsultasi tertunda di Klinis Dokter |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `13.2` |
| Trace | `RJ-DOC-DEC-029`, `030`, `031`; `RJ-DOC-FE-010`, `011`, `012` |
| Kontrak | `RJ-DOC-PENDCONS-001@1.0.0` (`approved`, `RJ-DOC-DEC-032`) |
| Desain | `03-frontend-architecture.md` *Amendment KT* (KT-FE.1–KT-FE.7); `04-prd-to-mvp.md` *Amendment KT* (`UAT-KT-01`..`07`) |
| Dependency | `[BE] RJ-DOC-REV-BE-012` ✅ |
| Wewenang | `RJ-DOC-DEC-033`; branch `sukmagpV2` ditetapkan pemilik pada sesi 5 Okt 2026 |
| Repository / branch | `V2QuilvianSystemFrontendDev` @ `sukmagpV2` (`74fdda7fa`, upstream `origin/sukmagpV2`). Area terdampak identik dengan snapshot desain `d232feb2b`. Belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ SELESAI |

## 1. Perubahan

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `src/lib/services/health-services/registration-management/doctor-queue.service.js` | Diperbarui | `getDoctorPendingConsultations(params, config)` → `GET /doctor-queues/pending-consultations` (`pageNumber`, `pageSize`, `queueId`) |
| `src/utils/health-services/registration-management/doctor-queue/doctor-pending-consultation-utils.js` | Baru | `isPastQueueItem`, `getPendingDays`, `getDraftPrescriptionCount`, `getProcedureCount`, `canCancelPendingConsultation`, `getPendingConsultationId`, `formatQueueDateLabel` |
| `src/lib/hooks/health-services/registration-management/doctor-queue/useDoctorPendingConsultations.js` | Baru | Daftar tertunda (muat, muat ulang ber-debounce mengikuti `refreshKey`, permintaan lama dibuang), `fetchLatestItem(queueId)`, alur batal konsultasi (`openCancel`, `closeCancel`, `confirmCancel`) |
| `src/components/features/health-services/doctor-queue-features/QueuePatientCard.jsx` | Diperbarui | Prop opsional `pendingMode`, `pendingDays`: footer "Tertunda N hari" + tombol Buka, tanpa Panggil/Lewati/Tidak Hadir. Default tidak berubah |
| `src/components/features/health-services/doctor-queue-features/FinalizeConsultationPanel.jsx` | Diperbarui | Prop opsional `secondaryAction` di samping tombol Simpan. Default tidak berubah |
| `src/components/features/health-services/doctor-queue-features/FinalizeConsultationModal.jsx` | Diperbarui | Prop opsional `pastVisitConfirmation`: `BaseCheckboxCard` wajib dicentang sebelum Simpan bila kunjungan lampau memuat resep draf atau tindakan. Default tidak berubah |
| `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx` | Diperbarui | Wilayah (A) banner, (B) bagian Konsultasi tertunda, (C) tombol Batalkan Konsultasi, (D) konfirmasi Simpan dengan baca ulang hitungan, (E) `ConfirmModal` batal. `activeItem` dari gabungan antrean hari ini + daftar tertunda. Daftar tertunda dimuat ulang sesudah Simpan |
| `src/style/health-services/registration-management/doctor-queues/doctor-queue-view.module.css` | Diperbarui | 10 class baru (`leftPanel_withPending`, `pendingSection*`, `pendingList`, `pendingStateText`, `pendingDaysText`, `finalizeConsultationActions`), seluruhnya token |

Diff: 6 berkas `+388 / -7`, ditambah 2 berkas baru. Tiga berkas yang sebelumnya termodifikasi di
`QuilvianDevV2` tidak ada di branch ini dan tidak disentuh.

**Delta terhadap desain:** state daftar tertunda ditempatkan di hook baru
`useDoctorPendingConsultations.js`, bukan di `use-doctor-queue.js` (1.788 baris) seperti tertulis di
`03` KT-FE.6. Perilakunya sama: dimuat ulang setiap kali antrean hari ini dimuat ulang (event
realtime dan aksi dokter), dengan jeda 650 ms yang sama.

## 2. Gerbang keputusan base component

Modul referensi visual: Klinis Dokter (`doctor-queues`) itu sendiri; pola khusus workspace dua panel.

`UI GATE: 7 elemen — REUSE 3, EXTEND 3, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| (A) Banner kunjungan lampau | `InformationAlert` | `base-features/information-alert.jsx`, sudah dipakai view ini | REUSE | `variant="warning"` |
| (B) Bagian Konsultasi tertunda | class panel view + `QueuePatientCard` + `BaseButton` | `base-features/base-button.jsx` | COMPOSE | Dirangkai di view |
| (B) Kartu tertunda | `QueuePatientCard` (domain) | 1 pemakai (view ini) | EXTEND | `pendingMode` opsional |
| (C) Tombol Batalkan Konsultasi | `FinalizeConsultationPanel` + `BaseButton` | 1 pemakai | EXTEND | `secondaryAction` opsional, `BaseButton variant="danger"` |
| (D) Konfirmasi Simpan | `FinalizeConsultationModal` + `BaseCheckboxCard` | `base-features/base-checkbox-card.jsx`, dipakai `journal-reversal-dialog` | EXTEND | `pastVisitConfirmation` opsional |
| (E) Modal batal | `ConfirmModal` | `requireReason`, `maxReasonLength` sudah ada | REUSE | `maxReasonLength={250}` |
| Pesan hasil batal | `MessageBox` (domain) | sudah dipakai view ini | REUSE | — |

Pilihan yang disajikan saat gerbang (tanpa jawaban, opsi A dijalankan):

- **Kartu tertunda — A (rekomendasi, dipakai):** `pendingMode` di `QueuePatientCard`; tampilan sama dengan kartu hari ini. **B:** kartu baru, menduplikasi gaya kartu.
- **Tombol batal — A (rekomendasi, dipakai):** slot di panel Simpan. **B:** tombol di atas tab, lebih mudah tertekan tanpa sengaja.
- **Konfirmasi Simpan — A (rekomendasi, dipakai):** `BaseCheckboxCard` di modal yang ada. **B:** modal konfirmasi kedua, satu klik tambahan.

## 3. Validasi

| Perintah | Hasil |
| --- | --- |
| `npx eslint` pada 8 berkas | `0 error, 0 warning` (run pertama 2 warning `react-hooks/set-state-in-effect`, diperbaiki) |
| `npm run build` | `PASS` (Next.js 16.2.12) |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — test-policy; tidak menambah framework test |

### Checklist konsistensi UI (grep pada diff)

| Pemeriksaan | Hasil |
| --- | --- |
| Hex/rgb/hsl, `!important`, `prefers-color-scheme` baru | Tidak ada |
| `<button>` mentah, `.btn`, `<table>`, inline style baru | Tidak ada |
| Typography pada CSS baru | Hanya pada class baru milik fitur ini, seluruhnya token `--font-size-*`, `--font-weight-*`, `--line-height-*`; tidak menyasar base component |
| Nilai literal | `border: 1px` dan `max-height: 260px` pada daftar tertunda (tidak ada token tinggi daftar) |
| Teks | Bahasa Indonesia; label tombol memuat `loadingLabel` |

### Verifikasi manual (Playwright, `ui_fe012.cjs`)

Lingkungan: server dev frontend milik pemilik di `localhost:3000` (source `sukmagpV2` + perubahan ini).
Panggilan API `https://localhost:7184` dialihkan ke backend uji `https://localhost:7185` (build `BE-012`,
`BlockActiveEncounter=true`) hanya di browser uji. Login lewat layar sebagai `uji.rjdp.dokter` (role
SuperAdmin dicopot sementara). Data uji pasien `KSKTEST-RM-07`: `ENC-RSMMC-00202` (C) dan `00203` (D),
`QueueDate` dimundurkan dengan izin pemilik.

| ID | UAT | Skenario | Hasil |
| --- | --- | --- | --- |
| W1–W4 | `UAT-KT-01` | Bagian Konsultasi tertunda tampil dengan jumlah 1; kartu "Tertunda 5 hari" + Buka tanpa Panggil/Lewati; antrean hari ini tetap tampil; request `pending-consultations` terkirim | `PASS` |
| W5–W7 | `UAT-KT-01` | Buka item: workspace terbuka, banner "Kunjungan tanggal 30 Sep 2026 — tertunda 5 hari. Pasien mungkin sudah pulang…", tombol Batalkan Konsultasi tampil | `PASS` |
| W8–W10 | `UAT-KT-02` | Modal Simpan: "1 resep akan diteruskan ke farmasi dan 0 tindakan akan ditagihkan."; Simpan nonaktif sampai dicentang | `PASS` |
| W11 | — | Simpan dengan resep kosong | Ditolak backend "Resep belum memiliki obat umum atau bahan racikan."; pesan tampil di modal (validasi finalisasi lama, benar) |
| W13–W16 | `UAT-KT-02` | Resep kosong dibatalkan lewat API; Simpan lagi: tanpa kotak konfirmasi, Simpan aktif, konsultasi tersimpan, workspace tertutup, item hilang. DB: `00202` status 7 | `PASS` |
| X1–X5 | `UAT-KT-03`, `UAT-KT-05` | Batalkan Konsultasi: modal menyebut pasien dan tanggal; konfirmasi nonaktif tanpa alasan; sesudah alasan diisi berhasil; pesan "Konsultasi dibatalkan. Minta petugas membatalkan kunjungan ENC-RSMMC-00203 di Daftar Pasien Rawat Jalan."; item hilang | `PASS` |
| — | `UAT-KT-03` | Petugas membatalkan `00203` lewat `PATCH /outpatient-encounters/{id}/cancel` | `200` |
| Y1–Y2 | `UAT-KT-07` | `pending-consultations` dipaksa `500`: "Konsultasi tertunda gagal dimuat." + Coba lagi; antrean hari ini tetap tampil | `PASS` |
| Y3 | — | Tanpa konsultasi tertunda: bagian tidak tampil | `PASS` |

Ringkasan: `22/22 PASS` pada run akhir. Run pertama fase `save` berhenti di W3 karena selektor skrip
`aside` ambigu (sidebar navigasi juga `aside`); W11/W12 gagal karena resep kosong ditolak backend, lalu
diuji ulang sebagai W13–W16. Tangkapan layar `fe012-*.png` di scratchpad.

| UAT | Status |
| --- | --- |
| `UAT-KT-04` dokter lain tidak melihat | Dibuktikan di backend (`BE-012` S4/S5); frontend tidak menyaring sendiri. Uji layar dokter kedua `MANUAL TEST: NOT FEASIBLE` — tidak ada akun dokter kedua tanpa role SuperAdmin |
| `UAT-KT-06` bukan penulis ditolak | Penjaga backend lama (`EnsureSoleAuthorAsync`). Tampilan pesan penolakan di modal Simpan terbukti (W11). Uji layar dengan dokter bukan penulis `MANUAL TEST: NOT FEASIBLE` — alasan sama |
| Muat ulang lewat event realtime | Tidak diuji: WebSocket SignalR uji tetap ke server lama 7184 karena pengalihan hanya untuk HTTP. Badge "1 Issue" Next.js di tangkapan layar berasal dari kegagalan WebSocket ini, bukan dari kode task |

## 4. Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| `AT-KT-11` (`UAT-KT-01`..`07`) | Terbukti, kecuali `04`/`06` di layar (`NOT FEASIBLE`, terbukti di backend) | §3 |
| Skema `03` KT-FE.3 seluruh wilayah (A)–(E) | Terbukti | W1–W16, X1–X5 |
| Keadaan KT-FE.5: gagal, kosong, item aktif gabungan, sesudah Simpan/Batalkan, klik ganda | Terbukti, kecuali muat ulang realtime | Y1–Y3, W15–W16, X5; tombol nonaktif selama kirim (`loading`) |
| Antrean hari ini berperilaku sama | Terbukti | W3, Y2; default komponen domain tidak berubah |

## 5. Pemulihan data dan akun uji

| Butir | Keadaan |
| --- | --- |
| Role SuperAdmin `uji.rjdp.dokter` | Dikembalikan (baris `AspNetUserRoles` yang sama); akun kini memegang SuperAdmin + Supervisor seperti semula |
| Dokter uji dan pegawai `UJI-RJDP Tanpa Cakupan` | Bypass lokasi dimatikan, status nonaktif; login ditolak `401` |
| `ENC-RSMMC-00202` | Konsultasi selesai, status 7 (resep kosong uji dibatalkan) |
| `ENC-RSMMC-00203` | Konsultasi dibatalkan, kunjungan `IsCancel = true` |
| Backend uji 7185/5199 | Dihentikan |

## 6. Risiko tersisa

1. Server dev backend pemilik (7184) masih kode lama; restart agar layar memakai endpoint baru.
   Tanpa restart, bagian tertunda menampilkan "Konsultasi tertunda gagal dimuat." (404).
2. Muat ulang realtime belum diverifikasi langsung (lihat §3).
3. Risiko dari `BE-012` §7 tetap berlaku (role SuperAdmin akun uji, bug bypass lokasi).

## 7. Task berikutnya

Bagian 13 roadmap selesai. Langkah pemilik: restart server 7184, commit kedua repository, lalu
opsional `verify-module-readiness`.

## 8. Perbaikan 5 Okt 2026 — kartu tertunda tergencet

**Laporan pemilik:** dengan 9 konsultasi tertunda, kartu bertumpuk dan scrollbar berhenti di tengah
padahal isi sudah habis.

**Sebab:** `.pendingList` memakai `display: grid` dengan `max-height`. Kartu `.queueCard` memakai
`overflow: hidden`, sehingga tinggi minimum otomatisnya dihitung 0 dan baris grid ikut menyusut
mengikuti ruang yang ada. Uji sebelumnya hanya memakai 1 item, jadi gejala ini tidak muncul.

**Perbaikan:** `.pendingList` memakai layout blok seperti `.queueList` (jarak dari margin kartu,
margin kartu terakhir 0), `overflow-x: hidden`, `padding-right: var(--space-1)`. Hanya
`doctor-queue-view.module.css` yang berubah; grep anti-regresi tetap bersih.

**Verifikasi** (`ui_fe012_scroll.cjs`, baca-saja, server dev pemilik, 9 data nyata): tinggi kartu
127 px = tinggi alami 125 px + border (sebelumnya tergencet); `scrollTop` maksimum 1086 + tinggi
wadah 260 = `scrollHeight` 1346; tangkapan layar dasar daftar menampilkan kartu terakhir utuh.

## 9. Perubahan 5 Okt 2026 — daftar tertunda menjadi tab (`RJ-DOC-FE-013`)

**Permintaan pemilik:** antrean hari ini dan konsultasi tertunda dipisah dengan tab di atas teks
"Pasien Dokter".

**Perubahan:** `doctor-queue-view.jsx` memakai `ClinicalTabNav` (`components/ui/doctor-clinical-base`,
REUSE — sudah dipakai workspace dokter Rawat Inap, lengkap dengan relasi tab/panel dan navigasi panah).
Tab "Hari ini (N)" dan "Tertunda (N)"; bawaan Hari ini. Subjudul mengikuti tab. Tab Tertunda memakai
wadah `.queueList` yang sama sehingga mengisi tinggi panel penuh, dengan keadaan memuat, kosong
("Tidak ada konsultasi tertunda"), dan gagal + Coba lagi. CSS bagian terpisah (`pendingSection*`,
`pendingList`, `leftPanel_withPending`, `pendingStateText`) dihapus; diganti `leftPanel_withTabs` dan
`pendingErrorState` (token saja). Tab Hari ini, infinite scroll, dan kartu antrean tidak berubah.

`UI GATE` diperbarui: elemen (B) menjadi `REUSE ClinicalTabNav` + `COMPOSE` daftar; tidak ada `NEW`.

**Validasi:** ESLint `0 error, 0 warning`; `npm run build` `PASS`; `ui_fe012_tabs.cjs` (baca-saja,
server dev pemilik, SuperAdmin, 8 data nyata) `9/9 PASS`: dua tab di atas judul, bawaan Hari ini tanpa
kartu tertunda, panah kanan berpindah tab, 8 kartu tidak tergencet dan scroll sampai dasar, subjudul
berganti, Buka dari tab Tertunda membuka workspace dengan banner, pindah tab tidak menutup workspace.


# Laporan Perubahan Frontend — `FE-KSK-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-015` |
| Judul | Popup jadwal praktik di Layanan & Dokter |
| Roadmap | [frontend-roadmap.md](../../../roadmap/frontend-roadmap.md) — *Amandemen 8 Oktober 2026 (B)* |
| Trace | `RJ-DOC-DEC-072`, `076`; `03` *PM-FE.4* (blueprint Rawat Jalan) |
| Contract version | `GET /doctor-schedules/kiosk/options` (kontrak yang ada, tanpa perubahan) |
| Dependency | — |
| Task mode | `TASK MODE: FRONTEND` (`RJ-DOC-DEC-083`) |
| Commit frontend | `de323430` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Proses dari sisi pengguna

Di **Layanan & Dokter** (Pasien Lama dan Pasien Baru), bila poli yang dipilih tidak punya dokter yang praktik pada jam berjalan, keadaan kosong kini memuat tombol **Lihat Jadwal Praktik**. Tombol membuka popup **"Jadwal Praktik {poli}"** berisi jadwal minggu ini: hari (Senin lebih dulu), dokter dan spesialis, jam, sesi, dan ruang.

- Poli tanpa jadwal aktif sama sekali menampilkan "Poli ini belum punya jadwal dokter aktif".
- Popup ditutup dengan *Tutup*, ×, Escape, atau klik latar. Popup tidak memilih jadwal.

## 2. Temuan perilaku yang ada

Daftar poli Kiosk sudah hanya memuat poli yang **sedang buka**:

- Pasien Lama: `doctor-schedules/kiosk/available-clinics`.
- Pasien Baru: hasil `clinics/kiosk/options` pada jam uji hanya memuat Poli Umum dan Poli Jantung.

Akibatnya keadaan "poli tanpa dokter praktik" hanya muncul bila daftar dokter praktik-sekarang untuk poli itu kosong. Tombol dipasang tepat di keadaan kosong itu. Perilaku daftar poli tidak diubah.

## 3. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/kiosk/registration/kiosk-practice-schedule-utils.jsx` | **Baru** — `buildKioskPracticeScheduleRows` (jadwal mingguan + tanggal tertentu dalam 7 hari, urut hari lalu jam) |
| `src/lib/hooks/kiosk/registration/use-kiosk-practice-schedule.js` | **Baru** — muat jadwal poli tanpa saringan "praktik sekarang" (`fetchDoctorSchedules({ onlyKioskAvailableNow: false })`) |
| `src/components/view/kiosk/registration/shared/kiosk-practice-schedule-modal.jsx` | **Baru** — popup + tombol pemicu |
| `src/style/kiosk/registration/kiosk-practice-schedule-modal.module.css` | **Baru** — token global saja |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-service.jsx` | Tombol di keadaan kosong + popup |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-service.jsx` | Sama |

**UI GATE: 3 elemen — REUSE 0, COMPOSE 3, NEW 0**

| Kebutuhan UI | Keputusan | Status |
| --- | --- | --- |
| Dialog | Pola dialog Kiosk yang ada (`PayerSearchModal`: portal, Escape, klik latar) dan gaya bertoken `kiosk-inactivity-warning` | COMPOSE |
| Daftar jadwal | Baris kartu sentuh bertoken | COMPOSE |
| Tombol pemicu | Tombol garis primary ukuran sentuh | COMPOSE |

Pilihan dialog:

1. **(Rekomendasi, dipakai)** Rangkai pola dialog Kiosk yang sudah ada dengan token global. Konsisten dengan layar Kiosk (ukuran sentuh) dan tanpa risiko regresi.
2. Pakai `ConfirmModal` + `DataTable` dari admin. Gayanya admin (430 px, tipografi kecil), sehingga tidak cocok untuk layar sentuh Kiosk.

## 4. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` berkas baru + 2 step layanan | 0 error. Warning step layanan sama dengan `HEAD` (4 dan 3) | `PASS` |
| `npm run build` | exit 0 | `PASS` |
| Uji browser akun perangkat Kiosk (build 3100 → backend uji 7185, agent tiruan) | **5/5 PASS** | `PASS` |

Kredensial akun Kiosk diberikan lewat variabel lingkungan dan tidak ditulis ke berkas mana pun. Request tulis diblok (tidak ada data tersimpan).

| ID | Skenario | Hasil |
| --- | --- | --- |
| S1 | Pasien Baru, Poli Umum tanpa dokter praktik (respons "praktik sekarang" ditiru kosong) → tombol + popup memuat jadwal sungguhan (Senin dr. Rendy 08:00–12:00, …) | PASS |
| S3 | Tutup popup → tidak ada dokter "Dipilih" | PASS |
| S2 | Poli tanpa jadwal sama sekali (seluruh respons jadwal ditiru kosong) → "Poli ini belum punya jadwal dokter aktif" | PASS |
| S5 | Escape menutup popup | PASS |
| S4 | Pasien Lama, daftar dokter kosong → tombol + popup jadwal Poli Umum | PASS |

`AUTOMATED TEST: SKIPPED (opsional) — tidak ada test baru di repository.`

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Poli tanpa dokter praktik → tombol tampil dan popup memuat jadwal minggu itu | Terpenuhi | S1, S4 |
| 2. Poli tanpa jadwal sama sekali → "Poli ini belum punya jadwal dokter aktif" | Terpenuhi | S2 |
| 3. Popup dapat ditutup dan tidak memilih jadwal otomatis | Terpenuhi | S3, S5 |
| 4. Lint, build, uji browser akun Kiosk | Terpenuhi | Bagian 4 |

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Keadaan kosong diuji dengan respons tiruan karena daftar poli Kiosk hanya memuat poli yang sedang buka (bagian 2) |
| Coverage gap | Perangkat Kiosk fisik (layar sentuh) — UAT pemilik |
| Perubahan sampingan | `NONE` |

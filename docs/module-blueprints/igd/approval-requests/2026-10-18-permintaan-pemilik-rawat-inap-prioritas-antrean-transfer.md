# Permintaan ke Pemilik Rawat Inap — Prioritas Kamar untuk Pasien Member di Antrean Transfer

| Field | Value |
|---|---|
| `request_id` | `IGD-REQ-004` — rincian butir 5 [`IGD-REQ-003`](2026-10-18-permintaan-pemilik-rawat-inap-transfer-igd.md) |
| `tanggal` | 2026-10-18 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `penerima` | **Muhammad Hamzah** — PIC Admisi Rawat Inap & InPatient Management |
| `rujukan` | `IGD-DEC-316` (prioritas kamar berbasis membership; pemilik keputusan: pemilik IGD dan PIC Admisi Ranap; disetujui Rizki Gunawan, 16 Oktober 2026); `IGD-DEC-327` (sumber penanda: keanggotaan CRM dibaca langsung, 18 Oktober 2026); `IGD-DEC-309` (lencana CITO); [audit pasien prioritas dan billing IGD](../evidence/2026-10-18-audit-pasien-prioritas-dan-billing-igd.md) bagian 6; kontrak integrasi IGD `0.11.0` §10.4 |
| `status` | **`disetujui`** (Muhammad Hamzah, 19 Oktober 2026; rujukan blueprint `RWI-DEC-338` s.d. `341`) |
| `sifat` | Operasional, lintas modul. **Bukan** artefak desain IGD |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Demo Selasa 20 Oktober 2026 memperagakan alur pasien prioritas tujuh langkah. Langkah 6 — **prioritas kamar rawat
inap** — belum ada di aplikasi. Antrean Transfer IGD/Poli hari ini diurutkan menurut waktu keputusan saja
(`InpAdmissionTransferService.cs` `:46`) dan belum membaca CITO maupun keanggotaan pasien, sehingga petugas admisi tidak
tahu pasien mana yang perlu segera dipesankan kamar VIP/VVIP. Permintaannya satu: **urutkan antrean CITO → pasien member
prioritas → waktu, dengan lencana tier**. Datanya dibaca langsung dari CRM; IGD tidak berubah dan tidak ada migration.

_Contoh akibatnya hari ini._ Pukul 10.00 antrean berisi Ny. Sari (reguler, diputuskan 09.30), Tn. Hadi (member Platinum,
diputuskan 09.50), dan Tn. Joko (reguler, CITO, diputuskan 09.55). Antrean menampilkan Ny. Sari paling atas, lalu Tn. Hadi,
lalu Tn. Joko — pasien CITO justru paling bawah, dan Tn. Hadi tidak tampak sebagai pasien prioritas.

---

## 2. Yang TIDAK perlu diubah di Rawat Inap

| Hal | Tetap seperti sekarang |
|---|---|
| Isi Antrean Transfer | Saringan `TransferQuery` untuk form bertanda tangan (`IGD-DEC-307`) — siapa yang masuk antrean tidak berubah, hanya urutan dan lencananya |
| _Admit_ | `BE-RWI-207` dan perubahan `EmgDisposition` menjadi `Executed` |
| Data IGD | Tidak ada tabel `Emg*` yang ditulis; Form Permintaan Rawat Inap tidak diberi kolom baru (`IGD-DEC-316` (A) ditunda, `IGD-DEC-327`) |
| Triage IGD | Tetap tidak membaca membership (`IGD-DEC-315`) — prioritas member hanya berlaku untuk kamar, bukan penanganan medis |

---

## 3. Permintaan

| No | Permintaan | Letak | Dasar |
|---|---|---|---|
| 1 | **Baca penanda member prioritas.** Pasien dianggap member prioritas bila punya `MstPatientMembership` berstatus `Active`, hari ini berada di antara `PeriodStartDate` dan `ExpiredDate` (kosong = tanpa batas), dan tier-nya (`MstMembershipTier`) ber-`PriorityQueue` = `true`. Ambil juga `TierName` dan `PriorityLevel` | `InpAdmissionTransferService.Queries.cs` (`TransferQuery`) | `IGD-DEC-316` (B), `IGD-DEC-327` |
| 2 | **Baca CITO dari form IGD.** `EmgAdmissionRequest.IsCito` dari form bertanda tangan, lewat fungsi IGD `EmergencyDispositionService.BacaRingkasanPermintaanRawatInapAsync` yang memang disediakan untuk antrean (API IGD `0.19.0` §12.8) | Sama | `IGD-DEC-309` |
| 3 | **Urutan antrean:** CITO lebih dulu → member prioritas (`PriorityLevel` tertinggi lebih dulu) → waktu keputusan → id | `InpAdmissionTransferService.cs` `:46` (`OrderBy(x => x.DecidedAt)`) | `IGD-DEC-316` (B) |
| 4 | **Lencana di baris antrean:** lencana tier (`TierName`, misalnya _Platinum_) untuk member prioritas, dan lencana CITO | Tabel Antrean Transfer IGD/Poli (frontend Rawat Inap) | `IGD-DEC-316` (B), `IGD-DEC-309` |

_Contoh sesudah perubahan._ Antrean pukul 10.00 di atas menjadi: Tn. Joko (lencana **CITO**), Tn. Hadi (lencana
**Platinum**), Ny. Sari. Petugas admisi memesankan kamar untuk Tn. Joko lebih dulu, lalu memblokir kamar VIP untuk Tn. Hadi
sebelum melayani antrean reguler. Bila keanggotaan Tn. Hadi kedaluwarsa kemarin, ia diurutkan sebagai pasien reguler.

---

## 4. Untuk agent Claude di sisi Rawat Inap

Bila Mas Hamzah meminta agent Claude mengerjakan permintaan ini, agent tersebut:

1. Membaca berkas ini dan [audit pasien prioritas dan billing IGD](../evidence/2026-10-18-audit-pasien-prioritas-dan-billing-igd.md)
   bagian 6 lebih dulu.
2. Memasukkan permintaan ini ke blueprint `rawat-inap/episode-rawat-inap` lewat skill blueprint Rawat Inap — sebagai delta
   Antrean Transfer — dengan rujukan silang `IGD-REQ-004`, `IGD-REQ-003` butir 5, `IGD-DEC-316`, `IGD-DEC-327`.
3. **Tidak** menulis tabel `Emg*`, **tidak** menambah kolom pada Form Permintaan Rawat Inap IGD, dan **tidak** menyalin
   aturan status IGD ke Rawat Inap. CITO dibaca lewat fungsi IGD, bukan kueri langsung ke tabel IGD.
4. Mencatat jawaban di bagian 6 berkas ini, atau mengirimkannya ke Rizki untuk dicatat.

---

## 5. Urutan kerja dan uji bersama

| Langkah | Siapa | Isi |
|---|---|---|
| 1 | Rawat Inap | Butir 1–4 di atas. Untuk demo 20 Oktober 2026, idealnya dikerjakan Senin 19 Oktober bersama `IGD-REQ-003` butir 1 |
| 2 | Rizki | Build backend dan frontend yang memuat perubahan |
| 3 | IGD + Rawat Inap | Uji **P1-08** panduan uji IGD (`testing/2026-10-17-panduan-uji-sisa-modul-igd.md` bagian 3F): satu pasien CITO reguler, satu member Platinum, satu reguler di Antrean Transfer; urutan dan lencana sesuai bagian 3 |

---

## 6. Jawaban pemilik Rawat Inap

| Butir | Setuju? | Target / catatan |
|---|---|---|
| 1 — penanda member prioritas dari CRM | **Setuju** | Dibaca langsung dari `MstPatientMembership` & `MstMembershipTier.PriorityQueue = true` via `TransferQuery` (`RWI-DEC-338`, `RWI-DEC-339`) |
| 2 — CITO dari form IGD | **Setuju** | Dibaca via `EmergencyDispositionService.BacaRingkasanPermintaanRawatInapAsync` (`RWI-DEC-338`) |
| 3 — urutan antrean | **Setuju** | CITO → Member Prioritas (`PriorityLevel` tertinggi lebih dulu) → waktu keputusan (`DecidedAt`) → id (`RWI-DEC-338`) |
| 4 — lencana tier dan CITO | **Setuju** | Tampil di Tabel Antrean Transfer dan Header Ringkasan Admisi PPRI (`RWI-DEC-341`) |

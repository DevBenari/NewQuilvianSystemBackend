# Permintaan ke Pemilik Rawat Inap — Melengkapi Transfer Pasien IGD ke Rawat Inap

| Field | Value |
|---|---|
| `request_id` | `IGD-REQ-003` |
| `tanggal` | 2026-10-18 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `penerima` | **Muhammad Hamzah** — PIC Admisi Rawat Inap & InPatient Management |
| `rujukan` | `IGD-DEC-308`, `IGD-DEC-309` (disetujui Muhammad Hamzah, 15 Oktober 2026); [spesifikasi serah terima 15 Oktober](../evidence/2026-10-15-spesifikasi-serah-terima-rawat-inap-igd-dec-308-309.md); [penilaian B6 dan keselarasan](../evidence/2026-10-18-penilaian-b6-dan-keselarasan-rawat-inap.md); `IGD-FACT-127`…`129`; `IGD-OQ-132` |
| `status` | **`menunggu jawaban`** — butir 1–4. **Butir 5: dirinci sebagai [`IGD-REQ-004`](2026-10-18-permintaan-pemilik-rawat-inap-prioritas-antrean-transfer.md)** (18 Oktober 2026 malam), dikirim terpisah |
| `sifat` | Operasional, lintas modul. **Bukan** artefak desain IGD |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Uji transfer IGD → Rawat Inap tanggal 18 Oktober 2026 berhasil sampai pasien di-_admit_. **Cara Rawat Inap mengubah
tindak lanjut IGD menjadi `Executed` saat _admit_ (`BE-RWI-207`) sudah benar dan tidak perlu diubah.** Yang belum ada
di sisi Rawat Inap adalah empat hal di bawah. Dua di antaranya (butir 1 dan 2) sudah disepakati Mas Hamzah pada
15 Oktober, tetapi belum masuk blueprint `rawat-inap/episode-rawat-inap`, sehingga agent yang bekerja dari blueprint
Rawat Inap tidak akan mengerjakannya.

_Contoh akibatnya hari ini._ Pasien diantar ke bangsal dan dicatat tiba pukul 08.10, lalu petugas admisi menyelesaikan
_admit_ pukul 08.30. Karena tidak ada panggilan dari Rawat Inap sesudah _admit_, kunjungan IGD pasien itu tetap terbuka
dan berlencana "Menunggu keputusan" — bertentangan dengan roadmap Rawat Inap sendiri: _"Pasien tidak boleh berstatus
'Menunggu Kamar' lagi di IGD setelah admisi selesai."_

---

## 2. Yang TIDAK perlu diubah di Rawat Inap

| Hal | Tetap seperti sekarang |
|---|---|
| `BE-RWI-207` | _Admit_ membuka episode dan mengubah `EmgDisposition` menjadi `Executed` dalam satu transaksi |
| Arti `Executed` (kontrak `admission-transfer-repair-006.md` baris 13) | "Permintaan telah dipakai untuk membuka admisi dan memesan bed"; penutupan fisik IGD tetap lewat departure/arrival milik IGD |
| Antrean Transfer IGD/Poli | Saringan `TransferQuery` untuk form bertanda tangan (`IGD-DEC-307`) |

Perbaikan yang membuat kunjungan IGD tertutup dengan benar dikerjakan **di IGD** lewat `BE-IGD-083`. Begitu itu selesai,
panggilan pada butir 1 langsung berfungsi tanpa kerja ulang di sisi Rawat Inap.

_Tambahan 18 Oktober 2026 (siang)._ Dua hal yang ditangani `BE-IGD-083`, supaya sisi Rawat Inap tidak perlu menyesuaikan
apa pun: (1) fungsi IGD mengenali tindak lanjut yang baru diubah menjadi _Executed_ dalam transaksi yang sama walaupun belum
disimpan — jadi panggilan butir 1 memang diletakkan **sebelum** `SaveChangesAsync`, dan tidak perlu dipindah; (2) fungsi IGD
tidak menutup kunjungan selama pasien belum dicatat tiba di bangsal, sehingga pada urutan admisi dulu, lalu antar, panggilan
butir 1 tidak menutup kunjungan terlalu cepat. Keduanya urusan IGD (`IGD-FACT-131`, `132`).

---

## 3. Empat permintaan, ditambah usulan butir 5

| No | Permintaan | Letak | Dasar |
|---|---|---|---|
| 1 | **Panggil evaluasi penutupan IGD sesudah _admit_.** Tepat sesudah `EmgDisposition` menjadi `Executed`, dalam transaksi yang sama dan **sebelum** `SaveChangesAsync`: `await _emergencyDispositionService.EvaluasiPenutupanKunjunganSusulanAsync(disposition.EmergencyVisitId, actorUserId, DateTime.UtcNow, cancellationToken);` | `InpAdmissionTransferService.ExecuteTransferAdmissionCoreAsync`, blok `if (request.SourceType == "Emergency")` di baris 156–162 (snapshot `6bb18fe6`). _Spesifikasi 15 Oktober menyebutnya `AdmitAsync`; nama sebenarnya `ExecuteTransferAdmissionCoreAsync`._ | `IGD-DEC-308` |
| 2 | **Tombol dan endpoint _Tolak_ beralasan** di Antrean Transfer IGD/Poli. Endpoint milik Rawat Inap (usulan `POST /admission-transfers/{id}/reject`, alasan wajib) memanggil `BatalkanFormPermintaanOlehPetugasAdmisiAsync(dispositionId, reason, officerUserId, ct)`. Hanya form yang batal; tindak lanjut IGD tetap _Dikonfirmasi_ | Endpoint dan service Rawat Inap; tombol dan modal alasan di layar antrean | `IGD-DEC-309`; rincian pesan dan respons di spesifikasi 15 Oktober §2.1 dan §4 |
| 3 | **`BedManagement:UnitAssignments` memuat petugas admisi** di setiap lingkungan, **termasuk operasi `InpatientEpisode:Create`** (_tambahan 18 Oktober 2026 sore: tanpa operasi ini _admit_ ditolak `403` walaupun bed terlihat — terbukti di uji B6 dan B7, `IGD-FACT-134`_). Saat ini keempat entri milik satu pengguna, sehingga petugas admisi lain melihat papan bed kosong pada langkah _Pilih Bed_ dan tidak dapat _admit_, walaupun Akses Role-nya sudah lengkap | `appsettings` per lingkungan (dev, staging, produksi) | `IGD-FACT-127` |
| 4 | **Tetapkan nilai master kelas "KELAS II".** Semula `isForInpatient = false`, sehingga _admit_ ditolak `422`. Saat uji 18 Oktober, penguji mengubahnya menjadi `true` (izin pemilik IGD, `IGD-DEC-322`). Mohon tetapkan nilai yang benar | Master Data → Kelas Pasien | Temuan Putaran B6 |
| 5 _(18 Oktober 2026 malam — dirinci [`IGD-REQ-004`](2026-10-18-permintaan-pemilik-rawat-inap-prioritas-antrean-transfer.md))_ | **Urutkan Antrean Transfer IGD/Poli: CITO, lalu pasien member prioritas, lalu waktu — dengan lencana tier.** Rawat Inap membaca keanggotaan CRM aktif pasien langsung saat menyusun antrean; IGD tidak berubah. Rincian di bagian 7 | `InpAdmissionTransferService` (`TransferQuery`, urutan `:46`) dan tabel Antrean Transfer | `IGD-DEC-316` (B), `IGD-DEC-327`; fakta `IGD-FACT-143`; integration IGD `0.11.0` §10.4 |

---

## 4. Untuk agent Claude di sisi Rawat Inap

Bila Mas Hamzah meminta agent Claude mengerjakan permintaan ini, agent tersebut:

1. Membaca berkas ini dan [penilaian B6 dan keselarasan](../evidence/2026-10-18-penilaian-b6-dan-keselarasan-rawat-inap.md)
   bagian 3–6 lebih dulu.
2. Memasukkan butir 1 dan 2 ke blueprint `rawat-inap/episode-rawat-inap` lewat skill blueprint Rawat Inap — amandemen
   kontrak EPS-006 (`contracts/admission-transfer-repair-006.md`, `contracts/state-transition-matrix.md`) dan delta
   `BE-RWI-207` serta task baru untuk _Tolak_ — dengan rujukan silang `IGD-REQ-003`, `IGD-DEC-308`, `IGD-DEC-309`.
3. **Tidak** mengubah cara menulis `Executed`, **tidak** menulis tabel atau status kunjungan IGD (`EmgVisit`) secara
   langsung, dan **tidak** menyalin aturan status IGD ke Rawat Inap. Semua urusan status kunjungan IGD lewat fungsi IGD.
4. Memperlakukan butir 3 sebagai syarat rilis per lingkungan, dan butir 4 sebagai keputusan data master.
5. Mencatat jawaban di bagian 6 berkas ini, atau mengirimkannya ke Rizki untuk dicatat.

---

## 5. Urutan kerja dan uji bersama

| Langkah | Siapa | Isi |
|---|---|---|
| 1 | IGD | `BE-IGD-083` — fungsi IGD menutup kunjungan dengan benar sesudah tindak lanjut dijalankan admisi |
| 2 | Rawat Inap | Butir 1 (satu panggilan) dan butir 3 (penugasan bed petugas admisi) |
| 3 | IGD + Rawat Inap | Uji Putaran B7: alur normal (admisi dulu, lalu tiba) dan alur pasien-tiba-dulu, keduanya harus berakhir kunjungan IGD _Selesai_ |
| 4 | Rawat Inap | Butir 2 (_Tolak_), lalu uji B4-11…B4-16 dari panduan uji IGD |
| 5 | Rawat Inap, lalu IGD + Rawat Inap | Butir 5 (urutan antrean), lalu uji P1-08 dari panduan uji IGD. Untuk demo 20 Oktober 2026 butir ini dikerjakan bersama butir 1 pada Senin 19 Oktober bila Mas Hamzah setuju |

---

## 6. Jawaban pemilik Rawat Inap

| Butir | Setuju? | Target / catatan |
|---|---|---|
| 1 — panggilan sesudah _admit_ | | |
| 2 — _Tolak_ beralasan | | |
| 3 — `UnitAssignments` petugas admisi | | |
| 4 — nilai KELAS II | | |
| 5 — urutan antrean CITO → member prioritas → waktu (usulan) | **Disetujui via IGD-REQ-004** | Disetujui 19 Oktober 2026; rujukan `RWI-DEC-338` s.d. `341` pada berkas `IGD-REQ-004` |

---

## 7. Usulan butir 5 — prioritas kamar untuk pasien member di Antrean Transfer

_Ditambahkan 18 Oktober 2026 malam, sesudah audit pasien prioritas dan billing IGD
([rujukan tetap](../evidence/2026-10-18-audit-pasien-prioritas-dan-billing-igd.md), bagian 6). **Pada malam yang sama dijadikan
permintaan tersendiri [`IGD-REQ-004`](2026-10-18-permintaan-pemilik-rawat-inap-prioritas-antrean-transfer.md)**; jawaban Mas Hamzah
dicatat di bagian 6 berkas itu. Ringkasan di bawah dipertahankan sebagai konteks._

**Masalahnya.** Langkah 6 alur pasien prioritas — _prioritas kamar rawat inap_ — belum ada. Antrean Transfer IGD/Poli hari
ini diurutkan menurut waktu keputusan saja (`InpAdmissionTransferService.cs` `:46`) dan belum membaca CITO maupun
keanggotaan pasien. Akibatnya petugas admisi tidak tahu pasien mana yang perlu segera dipesankan kamar VIP/VVIP.

**Yang diminta.** Saat menyusun Antrean Transfer, Rawat Inap membaca keanggotaan CRM pasien langsung (`IGD-DEC-327`):

| Butir | Isi |
| --- | --- |
| Pasien member prioritas | Punya `MstPatientMembership` berstatus `Active`, hari ini di antara `PeriodStartDate` dan `ExpiredDate` (kosong = tanpa batas), dengan `MstMembershipTier.PriorityQueue` = `true` |
| CITO | Dari Form Permintaan Rawat Inap bertanda tangan (`EmgAdmissionRequest.IsCito`), dibaca lewat `EmergencyDispositionService.BacaRingkasanPermintaanRawatInapAsync` — fungsi IGD yang memang disediakan untuk antrean |
| Urutan | CITO lebih dulu → member prioritas (`PriorityLevel` tertinggi lebih dulu) → waktu keputusan → id |
| Tampilan | Lencana tier (`TierName`, misalnya _Platinum_) pada baris member; lencana CITO seperti `IGD-DEC-309` |
| Yang tidak berubah | Isi antrean (saringan `IGD-DEC-307`), _admit_ (`BE-RWI-207`), dan seluruh tabel `Emg*`. Triage IGD tetap tidak membaca membership (`IGD-DEC-315`) |

_Contoh._ Pukul 10.00 antrean berisi Ny. Sari (reguler, diputuskan 09.30), Tn. Hadi (Platinum, diputuskan 09.50), dan
Tn. Joko (reguler, CITO, diputuskan 09.55). Urutannya: Tn. Joko (CITO), Tn. Hadi (lencana _Platinum_), Ny. Sari. Petugas
admisi memesankan kamar untuk Tn. Joko lebih dulu, lalu memblokir kamar VIP untuk Tn. Hadi sebelum melayani antrean
reguler.

**Bukti selesai.** Uji P1-08 panduan uji IGD: urutan dan lencana sesuai tabel di atas, pada build yang memuat perubahan
ini.

**Untuk agent Claude di sisi Rawat Inap.** Aturan bagian 4 berlaku. Butir ini dimasukkan ke blueprint Rawat Inap
sebagai delta Antrean Transfer dengan rujukan silang `IGD-REQ-003` butir 5, `IGD-DEC-316`, dan `IGD-DEC-327`.
Penanda yang dibekukan di form IGD (`IGD-DEC-316` (A)) **tidak** diminta — keputusan pemilik IGD 18 Oktober 2026.

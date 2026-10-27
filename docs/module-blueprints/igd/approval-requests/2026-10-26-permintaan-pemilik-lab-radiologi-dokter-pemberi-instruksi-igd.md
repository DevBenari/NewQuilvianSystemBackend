# Permintaan ke Pemilik Laboratorium dan Radiologi — Aturan *Dokter Pemberi Instruksi* untuk Pesanan IGD

| Field | Value |
|---|---|
| `request_id` | `IGD-REQ-007` |
| `tanggal` | 2026-10-26 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `penerima` | **Yoga Aji Pratama** — pemilik Radiologi; **pemilik Laboratorium** (belum dipetakan namanya) |
| `rujukan` | `IGD-OQ-117` (dibuka 6 Oktober 2026), `IGD-DEC-224`, `IGD-FACT-082` |
| `status` | **`disetujui dengan perluasan syarat`** — Yoga Aji Pratama, pemilik Laboratorium dan Radiologi, 27 Oktober 2026 (`IGD-DEC-360`). *Sebelumnya:* **`menunggu jawaban`** |
| `sifat` | Lintas modul. **Bukan** artefak desain IGD — aturan yang diminta milik Laboratorium dan Radiologi |
| `menahan` | `BE-IGD-071` ⛔ dan `FE-IGD-058` ⛔; sebagian `FE-IGD-057` |

---

## 1. Satu paragraf untuk yang tidak punya waktu

`IGD-OQ-117` dibuka **6 Oktober 2026** dan sampai hari ini belum pernah ada dokumen permintaannya —
dua puluh hari itu bukan menunggu jawaban, melainkan belum ada yang ditanya. Permintaan ini menutup
kelalaian tersebut. Yang diminta satu hal: **apakah aturan *dokter pemberi instruksi* yang sekarang
hanya berlaku untuk pesanan rawat inap boleh diperluas ke pesanan pasien IGD**, dan kalau boleh,
siapa dokter yang sah serta apakah verifikasinya memakai status yang sama dengan rawat inap.

Tanpa jawaban ini dua task IGD berhenti total, dan perawat IGD tetap dapat memesan lab atau
radiologi **tanpa menyebut dokter mana pun** — pesanan klinis tanpa jejak siapa yang
menginstruksikan.

## 2. Keadaan sekarang, dikutip dari source

Bukan kesimpulan, melainkan isi berkasnya sendiri —
`Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` baris 127–133:

> *"Dokter pemberi instruksi — `BE-RWI-104`, `FR-DOK-106`. Hanya dibaca pada pesanan rawat inap
> (`InpEpisodeId` terisi) yang dibuat pengguna tanpa tautan dokter: wajib diisi dan wajib bertugas
> atas pasien. **Pesanan poliklinik dan IGD mengabaikannya**, sehingga pemanggil lama tidak berubah
> perilakunya."*

Jadi ruas `InstructingDoctorId` **sudah ada** pada kontrak pesanan lab, tetapi **sengaja diabaikan**
untuk encounter IGD. Itu keputusan Rawat Inap yang sah pada waktunya, dan kami tidak meminta
pembatalannya — kami meminta perluasannya.

## 3. Yang TIDAK kami minta

| Hal | Tetap seperti sekarang |
|---|---|
| Pemesanan oleh **dokter** langsung dari Ruang Kerja Dokter IGD | Sudah berjalan, tidak tertahan permintaan ini |
| Perilaku pesanan **poliklinik** | Tidak disentuh sama sekali |
| Perilaku pesanan **rawat inap** (`BE-RWI-104`) | Tidak diubah; kami mengikuti polanya, bukan menggantinya |
| Bank darah, hemodialisa, gizi | Ketiganya **sudah** mewajibkan ruas dokter peminta — di luar lingkup permintaan ini |

Yang tertahan **hanya** satu kaki: pesanan lab dan radiologi yang dibuat **perawat** atas instruksi
lisan dokter.

## 4. Tiga pertanyaan

| # | Pertanyaan | Kenapa perlu dijawab pemilik |
|---:|---|---|
| 1 | **Boleh atau tidak** `InstructingDoctorId` diwajibkan dan dibaca pada pesanan lab/radiologi ber-encounter IGD yang dibuat perawat? | Mengubah validasi milik Laboratorium dan Radiologi, bukan milik IGD |
| 2 | **Siapa dokter yang sah** sebagai pemberi instruksi untuk pasien IGD? Usulan IGD: dokter aktif yang memegang hak akses Ruang Kerja Dokter IGD — karena jadwal dokter jaga belum ada (`IGD-DEC-222`) | Rawat inap memakai *dokter yang bertugas atas pasien*; pasien IGD belum tentu punya DPJP saat pesanan dibuat |
| 3 | **Verifikasinya memakai status yang sama dengan rawat inap, atau status tersendiri?** | Menentukan apakah daftar *perlu verifikasi* di layar dokter IGD memakai mesin status yang sudah ada |

Bila jawabannya **tidak boleh**, mohon dinyatakan begitu saja — kami akan menutup `BE-IGD-071` dan
`FE-IGD-058` sebagai dibatalkan dan mencatat bahwa pesanan perawat IGD memang tidak menyebut dokter
pemberi instruksi. Jawaban "tidak" tetap jauh lebih berguna daripada tidak ada jawaban.

## 5. Akibat yang berjalan hari ini

*Contoh.* dr. Ani memimpin resusitasi dan meminta perawat Dimas memesan darah lengkap dan rontgen
toraks. Dimas memesannya dari layar perawat. Pesanan bank darah mencatat dr. Ani sebagai peminta —
karena bank darah memang mewajibkannya. Pesanan **radiologi** tidak mencatat siapa pun, dan dr. Ani
tidak punya daftar *perlu verifikasi* untuk meninjaunya. Bila kemudian dipertanyakan siapa yang
menginstruksikan rontgen itu, sistem tidak dapat menjawab.

## 6. Yang sudah disiapkan IGD

| Butir | Keadaan |
|---|---|
| Desain | `approved` sementara lewat `IGD-DEC-224`, pola `IGD-DEC-174` |
| Kartu task | `BE-IGD-071` dan `FE-IGD-058` sudah ditulis lengkap beserta acceptance-nya, tinggal dijalankan begitu jawaban tiba |
| Layar | `FE-IGD-057` (perawat memesan atas instruksi dokter) sudah dikerjakan 12 Oktober 2026; yang belum hanya kewajiban dan verifikasinya |

Begitu ketiga pertanyaan dijawab, IGD dapat mengerjakannya tanpa permintaan lanjutan.

## 7. Jawaban — Yoga Aji Pratama, 27 Oktober 2026

Yoga Aji Pratama memegang Laboratorium **dan** Radiologi (pernyataan pengaju 27 Oktober 2026), sehingga jawabannya
berlaku untuk keduanya. Ringkasan setia isinya:

1. **Disetujui.** `InstructingDoctorId` wajib dicatat pada pesanan Laboratorium dan Radiologi pasien IGD yang dibuat
   perawat atas instruksi dokter. Pesanan yang dibuat langsung oleh dokter tetap `NotRequired`.
2. **Dokter pemberi instruksi harus aktif dan memiliki kewenangan klinis yang sesuai.** Prioritaskan DPJP aktif atau
   dokter jaga IGD, tetapi pilihan harus mencerminkan dokter yang benar-benar memberi instruksi. Validasi di backend.
   Penugasan DPJP Rawat Inap tidak diberlakukan langsung pada pasien IGD.
3. **Disetujui memakai mekanisme verifikasi Rawat Inap yang sudah ada** — `NotRequired`, `Pending`, `Verified`, enum
   tetap milik masing-masing modul. Verifikasi oleh dokter pemberi instruksi lewat akun yang tertaut identitasnya.

Ketentuan implementasi: tidak mengubah alur Rawat Jalan maupun Rawat Inap; tidak membuat status verifikasi atau tabel
baru; identitas perawat penginput dan dokter pemberi instruksi dipertahankan terpisah; `Pending` tidak otomatis
menghambat pemeriksaan darurat (SOP klinis tetap berlaku); backend dan frontend IGD disesuaikan dan diuji bersama
sebelum dirilis.

**Tafsiran pengaju atas *kewenangan klinis*** (Rizki Gunawan, 27 Oktober 2026): *"dokter yang menangani atau memiliki
spesialis di bidangnya, seperti dokter radiologi, dokter yang menangani MCU, patologi klinik yang ada di lab, dll."*

**Akibat untuk IGD:** `IGD-OQ-117` dijawab (`IGD-DEC-360`). Butir 2 lebih sempit daripada desain yang terkunci
(*dokter aktif*, validation `0.14.0` §12.4 aturan 13), sehingga `BE-IGD-071` dan `FE-IGD-058` belum dapat
dikerjakan sebelum aturan itu diamandemen dalam bentuk yang dapat diperiksa backend (`IGD-OQ-144`).

# Sisa Pekerjaan Modul Rawat Inap

## 1. Identitas dokumen

| Atribut | Nilai |
|---|---|
| Tujuan | **Hanya memuat yang belum selesai.** Task yang belum dikerjakan, task yang baru sebagian, dan butir yang masih menggantung pada task yang sudah ✅ |
| Yang **tidak** ada di sini | Task yang sudah selesai. Untuk itu bacalah roadmap sub-modul masing-masing |
| Tanggal | 12 September 2026 |
| Blueprint | `RWI-BP-001` revision `6`, bentuk `COMPOSITE`, status `approved` |
| Backend | `NewQuilvianSystemBackend`, branch `MHamzah` |
| Frontend | `QuilvianSystemFrontendDev`, branch `HamzahV2` |
| Sifat dokumen | Catatan kerja. Ia **menurunkan** dari roadmap, tidak menciptakan task baru |

Bila dokumen ini berbeda dari roadmap sub-modul, **roadmap yang berlaku**. Letaknya
`<sub-modul>/roadmap/`.

---

## 2. ATURAN TETAP — cara menandai task backend selesai

> ### Task backend tidak perlu menunggu build
>
> **Begitu implementasi kodenya ada, task backend ditandai `SELESAI`.**
>
> `dotnet build`, `dotnet restore`, `dotnet test`, dan uji migration **tidak dijalankan** dan
> **tidak menahan** status task. Ini instruksi pemilik, berlaku terus, bukan pengecualian
> sekali pakai.
>
> **Yang tetap wajib:** butir verifikasi yang tidak dijalankan ditulis **`NOT RUN` apa adanya**
> pada laporan task — **jangan dihapus dari daftar**. Task boleh ✅ dengan butir `NOT RUN` di
> dalamnya; yang tidak boleh adalah butir itu hilang tanpa jejak.
>
> Aturan ini **tidak** berlaku untuk penerapan migration ke database. Itu tetap menuntut
> wewenang terpisah.

---

## 3. Ringkasan — apa yang tersisa

| Sub-modul | Backend | Frontend | Sisa |
|---|---|---|---|
| `episode-rawat-inap` | 40 dari 42 aktif | 24 dari 28 | **6 task** |
| `dokter-rawat-inap` | 22 dari 22 ✅ | 9 dari 9 ✅ | **nol task** |
| `keperawatan` | 14 dari 14 ✅ | 6 dari 6 ✅ | **nol task** |

**Seluruh sisa pekerjaan Rawat Inap kini terkumpul pada satu sub-modul,
`episode-rawat-inap`, dan terbagi dua kelompok:**

| Kelompok | Isi | Dapat dikerjakan sekarang? |
|---|---|---|
| A. Enam task terbuka | 1 sebagian, 5 tertahan | **Tidak.** Lima menunggu endpoint Billing yang nol barisnya ada |
| B. Butir menggantung pada task ✅ | Bukti, rilis, dan satu migration | Sebagian ya — lihat bagian 6 |

---

## 4. Kelompok A — enam task yang belum selesai

### 4.1 Satu task sebagian

#### 🟡 `FE-RWI-035` — Alur bisnis utama terbukti berjalan ujung ke ujung

**6 dari 8** acceptance criteria terpenuhi. Kriteria 8 ditutup 12 September 2026 setelah
`FE-RWI-039` selesai.

| Kriteria | Keadaan | Yang menahan |
|---|---|---|
| 1 | Sebagian — rangkaian sampai `Closed` terbukti, tetapi formulir pendaftaran pasien baru **tidak dikendarai** e2e | `RWI-UI-GAP-007`, ditambah `FilterDatePicker` dan lima pilihan wilayah berantai yang sulit dikendarai otomatis |
| 5 | Sebagian — **8 dari 19** layar punya e2e gerbang peran | Frontend **belum menerima katalog hak akses per butir** dari backend. Sebelas layar sisanya tidak punya penjaga di sisi layar untuk dibuktikan — cacat yang sama yang menahan `FE-RWI-003` kriteria 2 |

**Keduanya bukan pekerjaan koding pada task ini.** Kriteria 1 menunggu data pada lingkungan
target; kriteria 5 menunggu kontrak hak akses per butir dari backend.

### 4.2 Lima task tertahan — nol barisnya dapat ditulis

Kelimanya tertahan hal yang sama: **endpoint Billing yang belum ada di source.** Menulis kode
di atasnya berarti mengarang kontrak, dan itu dilarang.

| Task | Yang dibutuhkan | Bukti pemeriksaan source 12 September 2026 |
|---|---|---|
| 🟡 `FE-RWI-059` | `GET /patient-funds/deposit-policies` — `BE-BKC-039` | Pencarian `deposit-policies` pada seluruh `Areas/` backend: **nol hasil** |
| 🟡 `FE-RWI-061` | `GET /patient-funds/deposits/episodes/{episodeId}` — `BE-BKC-040` | Pencarian `deposits/episodes`: **nol hasil** |
| ⛔ `BE-RWI-071` | Sama, `BE-BKC-040` | Empat dari lima acceptance criteria tidak punya angka untuk diuji |
| ⛔ `BE-RWI-072` | Sama, `BE-BKC-040` | Kriteria 4 menuntut sistem membedakan "ringkasan tidak terbaca" dari "tidak ada kekurangan". Gerbang di atas sumber yang belum ada hanya punya dua kemungkinan, dan **keduanya merugikan**: menolak setiap penutupan episode, atau memperlakukan sumber yang belum ada sebagai lunas |

`BE-BKC-039` dan `BE-BKC-040` berada pada roadmap `billing-kasir` dengan status
`BLOCKED_PENDING_OWNER_APPROVAL`, menunggu `RWI-OQ-053` — **pemilik `BillingManagement` belum
bernama.**

#### 🟡 `FE-RWI-060` — tertahan hal yang berbeda, dan ini temuan baru

Kartu roadmap task ini berbunyi *"Backend-nya sudah siap apa adanya — lihat `RWI-FACT-018`"*.
**Pernyataan itu tidak akurat.**

| Hal | Keadaan sebenarnya |
|---|---|
| Route `POST /patient-funds/deposits/{encounterId}/top-ups` | **Ada**, di `BillingPatientFundsController.cs:93` |
| Badan permintaannya `DepositTopUpRequest` | Menuntut **`PaymentMethodId`** bertipe `Guid` **non-nullable** dan **`Reason`** ber-`[Required]` |
| Alur admisi rawat inap | **Nol** mengenal `paymentMethodId` — pencarian pada seluruh hook dan view `inpatient-management` mengembalikan nol hasil |

Baik scope `FE-RWI-060` maupun `FE-RWI-058` tidak menyediakan cara menangkap metode
pembayaran. Mengirim `Guid.Empty` atau memilih metode secara otomatis berarti **mencatat uang
atas metode yang tidak pernah dipilih petugas**, jadi nol baris ditulis.

**Yang dibutuhkan pemilik:** memutuskan di mana metode pembayaran ditangkap. Menambahkannya ke
langkah Deposit menuntut perubahan kriteria 4 `FE-RWI-058`, yang hari ini melarang langkah itu
mengirim permintaan jaringan apa pun.

---

## 5. Kelompok B — butir menggantung pada task yang sudah ✅

Task-nya sudah selesai dan tidak perlu dikerjakan ulang. Yang tersisa bukan kode.

### 5.1 Satu migration belum diterapkan — perlu perhatian

| Hal | Isinya |
|---|---|
| Migration | `20260909065125_AddInpatientEpisodeContextToPatientDiagnosis`, milik `BE-RWI-068` |
| Keadaan | **Belum diterapkan ke database mana pun** |
| Kenapa perlu perhatian | **Langkah mundurnya tidak simetris.** Mengembalikan `ConsultationId` menjadi `NOT NULL` akan **gagal** begitu ada satu baris diagnosis rawat inap tanpa nomor konsultasi |
| Urutan mundur yang benar | Kembalikan validasi lebih dulu, tangani baris yang telanjur ada bersama pemilik klinis, baru turunkan migration |
| Wewenang | Penerapannya menuntut **wewenang terpisah**. Aturan bagian 2 tidak mencakup ini |

### 5.2 Dua repository wajib rilis satu gelombang

| Task | Keadaan |
|---|---|
| `BE-RWI-073` | ✅ selesai, **masih lokal, belum di-commit** |
| `FE-RWI-062` | ✅ selesai, **masih lokal, belum di-commit** |

| Bila | Akibatnya |
|---|---|
| Backend turun lebih dulu | Tiga berkas test frontend gagal, karena menguji kode penolakan yang tidak pernah terbit lagi |
| Frontend turun lebih dulu | Petugas menerima penolakan tanpa pesan yang dapat dibaca |

Commit dan push dijalankan pemilik, bukan agent.

### 5.3 Butir verifikasi yang tercatat `NOT RUN`

Seluruhnya tercatat apa adanya pada laporan task masing-masing, sesuai aturan bagian 2.
**Tidak satu pun menahan status task.**

| Sumber | Isinya |
|---|---|
| Backend, seluruh task Gelombang 1A dan `BE-RWI-068` | `dotnet build` dan `dotnet test` `NOT RUN` atas instruksi pemilik |
| Backend, uji integrasi | `NOT RUN` — folder `Tests/` sudah tidak ada, dan `rules/backend/TEST_POLICY.md` melarang membuatnya kembali tanpa permintaan pemilik. Digantikan penelusuran source beserta contoh berangka |
| Frontend, bukti peramban | `NOT RUN` — repository **tidak memiliki `playwright.config.*`**, sehingga `npm run test:e2e` tidak dapat dijalankan |
| Frontend, bukti runtime | Tertahan `RWI-UI-GAP-007` |
| `BE-RWI-077` | Pembatalan tanda vital final `NOT RUN` — `ClinicalDocumentKind.VitalSign` belum ditegakkan mesin keutuhan dokumen. Dilacak `V2-UNK-01`, pemiliknya `MedicalRecordManagement` |
| `BE-RWI-078` | `AC-KEP-050` belum terpenuhi — peran nyata pada skenario negatif belum dapat dicatat |

### 5.4 Satu selisih dokumen yang perlu dibetulkan pemilik

| Dokumen | Yang perlu dibetulkan |
|---|---|
| Kartu `FE-RWI-058` pada `episode-rawat-inap/roadmap/frontend-roadmap.md` | Kriteria 1 menulis jalur pasien lama **sembilan** langkah dengan Deposit di urutan **keempat**. Jalur itu **sudah** sembilan langkah sebelum Deposit ada; angkanya disusun ketika masih delapan. Yang benar: **sepuluh langkah, Deposit di urutan kelima** |
| `episode-rawat-inap/03-frontend-architecture.md` bagian 3A.3 | Selisih yang sama. Barisnya menulis rentang "4–9" untuk **tujuh** butir |

Memaksakan angka sembilan menuntut menghapus satu langkah yang benar-benar dipakai.

---

## 6. Gerbang lingkungan dan approval yang masih terbuka

| ID | Isinya | Menahan apa | Cara menutupnya |
|---|---|---|---|
| `RWI-UI-GAP-007` | Data master rawat inap pada lingkungan target belum layak: pengaturan `DEFAULT` tidak ada, butir administrasi kosong, papan menunjukkan nol bed | Pembuktian runtime seluruh layar; `FE-RWI-035` kriteria 1 | **Menyiapkan data**, bukan menulis kode. Menanam data tiruan di frontend **dilarang** |
| `RWI-OQ-053` | Pemilik `BillingManagement` belum bernama | `BE-BKC-039`, `BE-BKC-040`, dan karenanya lima task pada bagian 4.2 | Menunjuk pemiliknya |
| `V2-UNK-01` | Sembilan jenis dokumen belum ditegakkan mesin keutuhan | Kelengkapan jalur pengganti `BE-RWI-077` | Pemilik `MedicalRecordManagement` |
| Approval `FE-INP-20` | Skema langkah Deposit ditulis 12 September 2026 pada `episode-rawat-inap/05-skema-tampilan.md` bagian 3.5A, revision `0.5`, status `draft` | Tidak menahan `FE-RWI-058` yang sudah ✅, tetapi bentuk layarnya **belum terkunci** | Persetujuan pemilik |
| Katalog hak akses per butir | Frontend belum menerimanya dari backend | `FE-RWI-035` kriteria 5; `FE-RWI-003` kriteria 2 | Kontrak baru dari backend |

---

## 7. Kemampuan yang belum menjadi task sama sekali

Sembilan kemampuan yang ditemukan audit 11 September 2026. **Belum boleh diturunkan menjadi
task**, jadi jangan mencari nomornya di roadmap — nomornya memang belum ada.

| Kemampuan | Status audit | Catatan |
|---|---|---|
| Medication Administration Record | `Missing` | Nol model, nol service, nol endpoint |
| Observasi transfusi dan reaksi | `Missing` | Yang ada hanya jenis consent dan alasan bank darah |
| Sliding scale | `Missing` | Menuntut protokol berversi yang juga belum ada |
| Handover antar shift keperawatan | `Missing` | Yang ada hanya serah terima kasir |
| Clinical handover transfer antarunit | `Missing` | Transfer bed atomik sudah ada, artefak klinisnya belum |
| Adverse drug reaction | `Missing` | Bergantung pada MAR |
| Intake, output, drain, dan WSD | `Reuse with adapter` | Pola sudah berjalan di IGD dan layak jadi rujukan bentuk |
| Lima cara keluar | `Extend` | Enum baru punya tiga nilai; `Death` dan `Absconded` menunggu sign-off klinis |
| Bukti consent per episode | `Extend` | Yang kurang hanya versi template dan penegakan keutuhan |

**Langkah yang tepat untuk kelompok ini bukan perencanaan, melainkan
`requirement-completeness-gate` lebih dulu.** Sepuluh butir `OPEN-MVP-001` sampai
`OPEN-MVP-010` pada PRD V2 masih terbuka, dan `RWI-DEC-097` **tidak** membukanya.

**Kelayakan keuangan berdiri sendiri.** Ia tetap `P0`, berstatus `external dependency`, dan
**bukan** `P1`. Sesuai `RWI-DEC-102`, selama kontrak authoritative Billing belum ada:
`AC-MVP-021` sampai `AC-MVP-023` belum boleh dinyatakan lulus, penandaan manual **tidak sah**
sebagai dasar penutupan episode normal di produksi, dan kesiapan MVP penuh belum boleh
dinyatakan selesai.

---

## 8. Catatan operasional

| Hal | Ketentuannya |
|---|---|
| **Task backend** | **Tanpa build. Implementasi kode selesai berarti task ✅.** Lihat bagian 2 |
| Butir yang tidak dijalankan | Ditulis **`NOT RUN` apa adanya** pada laporan, **bukan dihilangkan** |
| Skill yang dipakai | `build-module-backend` untuk task backend; `build-module-frontend` untuk task frontend |
| Wewenang | Setiap pemanggilan butuh **task ID** dan **wewenang tulis yang disebut eksplisit** |
| Repo frontend | Sesi terpisah di `QuilvianSystemFrontendDev`. Wewenang tulis backend tidak berlaku di sana |
| Penerapan migration | **Menuntut wewenang terpisah.** Jangan menerapkan ke environment mana pun tanpa itu |
| Git | Commit dan push dijalankan pemilik, bukan agent |
| Penutupan task | Selesai satu task berarti laporan ditulis **dan** register pada roadmap ikut diperbarui |

---

## 9. Nomor yang tidak boleh dipakai ulang

Nomor task yang sudah dipensiunkan: `BE-RWI-041` sampai `BE-RWI-043`, dan `FE-RWI-042` sampai
`FE-RWI-045` pada sisi `episode-rawat-inap`.

**ID bebas berikutnya: `BE-RWI-079` dan `FE-RWI-063`.**

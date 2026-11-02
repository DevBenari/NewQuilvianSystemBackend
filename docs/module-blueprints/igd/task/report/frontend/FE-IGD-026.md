# Laporan Susulan Frontend — `FE-IGD-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-026` |
| Judul | Laporan susulan layar pendaftaran IGD (`c8613d88c`) |
| Slice | `IGD-S02`, `IGD-S03` · `EPIC IGD-01`, `EPIC IGD-02` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.7 |
| Requirement | `FR-IGD-001`…`012` sisi tampilan — **dibaca, tidak diubah** |
| Keputusan | `IGD-DEC-084`, `IGD-DEC-107` |
| Kontrak | **Nol perubahan** |
| Klasifikasi | `REPORT` — **nol perubahan source** |
| Source yang dibaca | frontend `RizkiV2` `c4bcc39ac`; backend `rizkiG` `ed5cb292` |
| Tanggal | 26 Oktober 2026 |
| Status | ✅ **SELESAI — acceptance 1–4 terpenuhi; `git diff` source kosong** |

---

## 1. Acceptance 1 — ketiga commit dan pembuatnya

| Commit | Tanggal | Pembuat | Pesan | Berkas pendaftaran IGD |
| --- | --- | --- | --- | --- |
| `40f0e6106` | 29 Agustus 2026 | **MHamzah1** (tim Rawat Inap) | *"feat(inpatient-admission): enhance admission flow with patient type selection and new styles"* | `emergency-registration-stepper.jsx` (39 baris) |
| `5bc96f09b` | 29 Agustus 2026 | **Rizki0720** | *"merge dengan branch hamzah"* | `emergency-registration-stepper.jsx` (39 baris, lewat merge) |
| `c8613d88c` | 30 Agustus 2026 | **Rizki0720** | *"update"* | 4 berkas, **+19 / −64** |

### 1.1 Dua commit 29 Agustus — milik Rawat Inap, menyentuh berkas IGD

`40f0e6106` dibuat tim Rawat Inap untuk alur admisi, tetapi ikut **melahirkan**
`emergency-registration-stepper.jsx` — komponen stepper pendaftaran IGD. Isinya bukan perubahan
perilaku pendaftaran melainkan komponen stepper baru yang dapat dipakai ulang (`currentStep`,
`steps`, `ariaLabel`, penjaga `safeSteps` terhadap array kosong). `5bc96f09b` hanyalah merge Rizki
atas branch itu.

Dicatat di sini karena pola yang sama sudah pernah menjadi persoalan: berkas IGD disentuh dari
luar modul tanpa laporan, sehingga blueprint IGD tidak mengetahuinya. Pada kasus ini dampaknya
**netral** — komponennya aditif dan tidak mengubah validasi mana pun.

### 1.2 `c8613d88c` — empat berkas

| Berkas | Perubahan | Sifat |
| --- | --- | --- |
| `emergency-visit-step.jsx` | −47 | **Menghapus blok validasi yang sudah berkomentar mati** dan mengomentari ringkasan pasien |
| `payment-method-step.jsx` | −26 | Ganti nama kelas CSS (`sectionTitleRow` → `flowHeaderRow`, `secondaryButton` → `primaryButton`) dan menghapus blok ringkasan pasien |
| `verification-step.jsx` | 2 | Satu baris |
| `emergency-registration.module.css` | +8 | Kelas baru pendukung |

**Koreksi penting atas dugaan kartu.** `c8613d88c` **bukan** commit yang menonaktifkan validasi — ia
commit **pembersihan** yang menghapus kode yang sudah dikomentari mati sebelumnya. Yang
menonaktifkannya adalah `1681b1737`, **7 Agustus 2026, Rizki Gunawan**, *"memperbaiki tampilan
pendaftaran pasien IGD"*, dengan komentar yang menyatakan maksudnya terang-terangan:

> *"**TEMPORARY:** Seluruh required Emergency Visit sementara dinonaktifkan agar testing flow dapat
> langsung menuju langkah Pembayaran."*

## 2. Acceptance 2 — validasi layar yang dinonaktifkan, dibandingkan dengan validation §1

| # | Validasi yang dimatikan | Pesan aslinya | Padanan di validation §1 |
| ---: | --- | --- | --- |
| 1 | Penjaga master masih dimuat | *"Master Cara Kedatangan dan Jenis Kasus masih dimuat."* | **Tidak ada.** Murni kenyamanan layar, bukan aturan bisnis |
| 2 | Penjaga master kosong | *"Master Cara Kedatangan atau Jenis Kasus belum tersedia. Muat ulang data terlebih dahulu."* | **Tidak ada** sebagai aturan §1 |
| 3 | Pasangan tanggal dan jam trauma | *"Tanggal trauma dan jam trauma harus diisi berpasangan atau dikosongkan keduanya."* | **Tidak ada** — lihat bagian 3.2 |
| 4 | Seluruh penanda `required` Emergency Visit | — (penanda, bukan pesan) | Tidak satu pun ruas Emergency Visit diwajibkan §1 |

**Keadaan hari ini, 80 hari kemudian: keempatnya masih mati.** `emergency-visit-step.jsx` memuat
**tepat satu** aturan `required`, dan itu pun bukan salah satu di atas melainkan tambahan
belakangan dari `FE-IGD-034`: *Alasan Pendaftaran Episode Ganda* (`:170`), yang pesannya
*"Alasan pendaftaran episode ganda wajib diisi karena pasien masih memiliki kunjungan IGD yang
berjalan."*

Penelusuran `optionLoading`, `arrivalModeOptions.length === 0`, dan `Boolean(traumaDate) !== Boolean(traumaTime)`
pada berkas itu menjawab **nol hasil**. Ketiganya tidak dikembalikan, hanya dihapus.

### 2.1 Contoh yang disebut kartu — dan koreksinya

Kartu mencontohkan *"komentar `TEMPORARY: required dinonaktifkan` pada alias sementara pasien,
padahal §1 aturan 3 mewajibkan nama sementara"*. Pemeriksaan menunjukkan contoh itu **tidak tepat
sasaran**: komentar `TEMPORARY` berada pada ruas-ruas **Emergency Visit** (cara kedatangan, jenis
kasus, data trauma), **bukan** pada alias sementara pasien. Alias sementara berada di langkah
Pasien Baru, yang diatur `FE-IGD-070` dan `IGD-DEC-339`.

Substansi kekhawatirannya tetap sah dan dijawab bagian 3: apakah ada aturan §1 yang kehilangan
penjaganya karena layar berhenti mewajibkan.

## 3. Acceptance 3 — apakah backend tetap menegakkannya

### 3.1 Cara kedatangan dan jenis kasus — backend **tidak** mewajibkan

`EmergencyVisitService.cs`:

| Ruas | Keadaan |
| --- | --- |
| `ArrivalModeId` | `Guid?` **nullable** (`:75`). Diperiksa **hanya bila diisi**: `if (request.ArrivalModeId.HasValue && …Value != Guid.Empty && !await …AnyAsync(…))` → *"ArrivalModeId tidak ditemukan."* (`:263`–`:268`) |
| `CaseTypeId` | `Guid?` **nullable** (`:76`), pola persis sama (`:270`–`:274`) |

Jadi penolakannya hanya untuk **id yang salah**, tidak pernah untuk **id yang tidak ada**.
Pendaftaran IGD tanpa cara kedatangan dan tanpa jenis kasus **diterima server**. Matinya penjaga
layar karena itu **tidak melanggar kontrak** — kontraknya memang tidak mewajibkan.

Konsekuensi yang perlu dinyatakan: laporan yang mengelompokkan kunjungan menurut cara kedatangan
atau jenis kasus akan memuat baris tanpa kelompok, dan tidak ada satu lapisan pun yang
mencegahnya hari ini.

### 3.2 Pasangan tanggal dan jam trauma — tidak punya padanan backend

Backend menyimpan **satu** ruas gabungan `DateTime? WaktuTrauma` (`:83`), bukan tanggal dan jam
terpisah. Aturan *"harus berpasangan"* adalah konsep **khusus layar** yang lahir dari dua input
terpisah di UI. Dengan satu ruas gabungan, keadaan "tanggal ada tetapi jam kosong" tidak dapat
terwakili di server — ia hanya dapat lahir di layar.

Artinya matinya penjaga itu memindahkan risikonya, bukan menghapusnya: layar dapat mengirim
`WaktuTrauma` berisi tanggal dengan jam `00:00` tanpa petugas menyadarinya.

### 3.3 Aturan §1 yang **tetap** dijaga backend

Supaya laporan ini tidak terbaca sebagai daftar kebocoran, satu aturan §1 yang paling dekat dengan
kekhawatiran kartu **memang ditegakkan server**:

| Aturan §1 | Penegakan |
| --- | --- |
| Nomor 3 — *"Pasien tanpa identitas wajib punya nama sementara"*, `400` | **Ada di source**: `EmergencyVisitService.cs:895` menerbitkan persis *"Nama sementara wajib diisi untuk pasien yang belum diketahui identitasnya."* |

Jadi jalur pasien tanpa identitas tidak bergantung pada penanda `required` layar.

## 4. Ringkasan untuk pemilik

| Butir | Keadaan |
| --- | --- |
| Validasi yang dimatikan 7 Agustus 2026 | **Masih mati**, 80 hari |
| Dua di antaranya | Murni kenyamanan layar — tidak ada aturan bisnis yang hilang |
| Pasangan waktu trauma | Hilang tanpa padanan server (bagian 3.2) |
| Cara kedatangan dan jenis kasus | Tidak pernah diwajibkan kontrak; pendaftaran tanpa keduanya sah |
| Nama sementara pasien tanpa identitas | Tetap dijaga server |

**Tidak ada yang diperbaiki pada task ini** — ia task laporan. Bila pemilik menghendaki penjaga
layar dikembalikan, yang paling layak lebih dulu adalah pasangan waktu trauma (bagian 3.2), karena
hanya itu yang tidak punya lapisan kedua.

## 5. Acceptance 4 — nol perubahan source

| Pemeriksaan | Hasil |
| --- | --- |
| `git diff` frontend | **kosong** |
| `git diff` backend | **kosong** |
| Berkas yang ditulis | Laporan ini, baris status roadmap, traceability |

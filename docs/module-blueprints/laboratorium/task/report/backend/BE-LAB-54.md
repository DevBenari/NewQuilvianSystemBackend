# Laporan Perubahan Backend — `BE-LAB-54`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-54` (backend) |
| Judul | `finalize`, `reopen`, dan `consultation` hasil Mikrobiologi — gelombang `MVP-7b`, slice `S4b` |
| Trace | `LAB-DEC-097`, `LAB-DEC-106`; menutup `ARCH-GAP-LAB-04` |
| Kontrak | `LAB-API-v1` `r26` bagian 21.2; `LAB-VAL-v1` `r9` (`VAL-107`, `VAL-108`); `LAB-PERM-v1` rev 8 bagian 10.1 |
| Klasifikasi | `MEDIUM` — tiga endpoint tulis; nol rilis, nol status hasil |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-21 |
| Dependency | `BE-LAB-53` — ✅ selesai |
| Status | ✅ **`SELESAI`** — ketiga endpoint berjalan; **`AC-159` dan `AC-169` terbukti terhadap aplikasi yang berjalan**; `AC-158` terbukti **sebagian**, dan sisanya diblokir `LAB-COORD-011` |

---

## 1. Satu cacat ditemukan pengujian, bukan pembacaan

Build bersih, pembacaan kode lolos, dan tiga dari empat jalur uji lewat. **Jalur kelima
menjawab `500`.**

`POST /reopen` pada hasil yang sah gagal dengan `Npgsql 23503` — **pelanggaran foreign key**
saat menyisipkan `LabTransitionHistory`.

**Sebabnya:** kueri pemeriksaan hanya meng-`Include` `Procedure`, sehingga
`examination.LabOrder` bernilai `null` dan `EncounterId` jatuh ke `Guid.Empty`.

> **Ini persis jebakan yang didokumentasikan entity ini sendiri** — dan arah bahayanya justru
> terbalik. Komentar pada `ResultEnteredByUserId` memperingatkan bahwa kolom **tanpa** foreign
> key akan menerima `Guid.Empty` **diam-diam**. `EncounterId` **punya** foreign key, sehingga
> database menolaknya dengan galat. Yang ini jauh lebih beruntung: ia berteriak.

**Diperbaiki:** `.Include(x => x.LabOrder)` ditambahkan, dan `EncounterId` dibaca langsung
tanpa jalan mundur `?? Guid.Empty` — jalan mundur itulah yang menyembunyikan kesalahannya.

**Efek samping yang menguntungkan:** kegagalan itu sekaligus membuktikan **rollback**.
Pemeriksaan database sesudah `500` menunjukkan `FinalizedAt` **masih terisi** dan
`ReopenCount` **masih `0`** — nol perubahan sebagian tersimpan.

---

## 2. Yang dibangun

| Bagian | Isi |
|---|---|
| Endpoint | `POST /{id}/result/microbiology/finalize`, `POST /{id}/result/microbiology/reopen`, `PUT /{id}/result/microbiology/consultation` |
| Service | `FinalizeMicrobiologyResultAsync`, `ReopenMicrobiologyResultAsync`, `RecordConsultationAsync`, `BuildCompletionResponse` |
| DTO | `LabReopenRequest`, `LabConsultationRequest`, `LabExaminationCompletionResponse` |
| Hak akses | `LabExamination : Update` dipakai ulang — **nol resource permission baru** |

### `IsReleased` dan `DeliveryBlockedReason` — dua ruas yang tampak berlebihan

Keduanya **selalu** bernilai "belum dirilis" pada rilis ini, dan itu disengaja.

> `LAB-DEC-097` menetapkan `Simpan Final` **bukan** rilis. Tanpa kedua ruas ini, pemanggil
> harus **menyimpulkan** hal itu sendiri dari ketiadaan endpoint rilis — dan kesimpulan yang
> paling mudah diambil justru yang salah: *"sudah final berarti sudah bisa dikirim"*.
>
> Ruas ini membuat jawabannya **dinyatakan**, bukan disimpulkan.

---

## 3. Verifikasi — terhadap aplikasi yang berjalan

Aplikasi dijalankan pada `http://localhost:5107` terhadap `QuilvianNewDevYoga`. Autentikasi
memakai akun `superadmin` lewat `POST /api/v1/Auth/login`; token dibawa sebagai **HttpOnly
cookie**, bukan bearer.

| # | Skenario | Diharapkan | Hasil |
|---|---|---|---|
| 1 | `finalize` pada hasil yang **belum diisi** | `422` | ✅ `422` — *"Hasil pemeriksaan ini belum diisi…"* |
| 2 | `reopen` pada hasil yang **belum pernah** final (`VAL-107`) | `422` | ✅ `422` — *"…tidak ada yang perlu dibuka kembali."* |
| 3 | `consultedAt` **masa depan** (`VAL-108`) | `422` | ✅ `422` — *"…tidak boleh melewati waktu sekarang."* |
| 4 | Konsultasi **sah** | `200` | ✅ `200`, tiga fakta tersimpan |
| 5 | `finalize` **sah** | `200` | ✅ `200`, `isFinalized: true` |
| 6 | `finalize` **dua kali** | `409` | ✅ `409` — *"…sudah dinyatakan selesai."* |
| 7 | `reopen` **tanpa alasan** | `422` | ✅ `422` — *"Alasan membuka kembali wajib diisi."* |
| 8 | `reopen` **sah** | `200` | ⚠ `500` → **diperbaiki** → ✅ `200` |

### Keadaan database sesudahnya

```text
FinalizedAt        = (kosong)
FinalizedByUserId  = (kosong)
ReopenCount        = 1
ConsultedToName    = Prof. dr. Usman Chatib Warsa, SpMK-K
ConsultedAt        = 20/09/2026 09:15:00
ConsultedByUserId  = 0ba84a1a-2559-49ba-a320-10fb1f399d70
```

Baris riwayat transisi terbaru:

```text
Action    = LabExamination.ReopenMicrobiologyResult
FromStatus= Finalized   ToStatus = Draft
ReasonNote= Koreksi pembacaan biakan hari ke-3
EncounterId sah (bukan Guid.Empty) = True
```

### Acceptance criteria

| AC | Hasil |
|---|---|
| `AC-159` | ✅ **Terbukti.** `reopen` mengosongkan `FinalizedAt` dan `FinalizedByUserId`, menaikkan `ReopenCount` ke `1`, dan **nol** baris koreksi `S6` tercipta — yang tertulis hanya riwayat transisi biasa |
| `AC-169` | ✅ **Terbukti.** Konsultasi menyimpan tiga fakta; `consultedByUserId` **diturunkan dari sesi**, bukan dari request; dan respons tetap `isReleased: false` — **nol** tombol pengiriman terbuka |
| `AC-158` | 🟡 **Terbukti sebagian.** `finalize` mengisi `FinalizedAt` + `FinalizedByUserId`, dan respons menyatakan hasil belum dirilis beserta sebabnya. **Bagian "ditolak ketika dicoba dikirim" TIDAK dapat diuji** |

### ⛔ Kenapa separuh `AC-158` tidak dapat diuji

**Nol jalur pengiriman hasil ada di kode.** Penelusuran 2026-09-21 menemukan nol kemunculan
`LabResultDelivery`, `ResultDelivery`, maupun endpoint pengiriman mana pun — pembangkit PDF dan
gerbang pesan keduanya masih `LAB-COORD-011`.

**Tidak ada yang bisa dicoba-kirim, sehingga tidak ada yang bisa menolak.** Yang dapat
dilakukan task ini sudah dilakukan: menyediakan `IsReleased` dan `DeliveryBlockedReason`
supaya jalur pengiriman kelak **tidak perlu menyimpulkan** — ia tinggal membacanya.

---

## 4. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Endpoint rilis | `S4d`, tertahan `DEC-LAB-011` |
| Ruas `resultQualifier`, `cultureType`, `susceptibilityMethod` pada request | Milik `PUT /result/microbiology` — task `BE-LAB-48` |
| Jalur pengiriman hasil | `LAB-COORD-011` |
| `git add`, `commit`, `push` | Nol diminta |

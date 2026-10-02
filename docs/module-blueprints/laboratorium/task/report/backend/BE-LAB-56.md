# Laporan Perubahan Backend — `BE-LAB-56`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-56` (backend) |
| Judul | Aturan kritis Mikrobiologi dan penilainya — gelombang `MVP-7`, slice `S4b` |
| Trace | `LAB-DEC-103`; mempersempit `INV-28`; menjawab bagian `LAB-OPEN-014` |
| Kontrak | `LAB-API-v1` `r26` bagian 21.3 dan 21.6; `LAB-VAL-v1` `r9` (`VAL-106`); `LAB-PERM-v1` rev 8 bagian 10.1 |
| Klasifikasi | **`HIGH`** — permukaan keselamatan pasien |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-21 |
| Dependency | `BE-LAB-53` ✅ |
| Status | ✅ **`SELESAI`** — **`AC-166`, `AC-167`, dan `VAL-106` terbukti terhadap aplikasi yang berjalan** |

---

## 1. Yang dibangun

| Berkas | Isi |
|---|---|
| `Models/LabMicrobiologyCriticalRule.cs` | Aturan per kombinasi organisme/antibiotik/interpretasi |
| `Repositories/Configurations/.../LabMicrobiologyCriticalRuleConfiguration.cs` | FK `Restrict`, index kombinasi |
| `DTOs/LabMicrobiologyCriticalRuleDtos.cs` | 4 DTO |
| `Services/LabMicrobiologyCriticalRuleService.cs` | CRUD + `LabCriticalRuleSet` penilai |
| `Controllers/LabMicrobiologyCriticalRuleController.cs` | 5 endpoint |
| `DTOs/LabMicrobiologyResultDtos.cs` | `CriticalRuleAvailable` dan `IsCritical` |
| `Services/LabMicrobiologyResultService.cs` | **Penilai disambungkan ke pembacaan hasil** |
| `Migrations/20260921054250_AddLabMicrobiologyCriticalRule.cs` | Dibangkitkan |

### Ketiga ruas penilai boleh kosong, dan kosong berarti "apa saja"

| Contoh | Bunyinya |
|---|---|
| `MRSA`, —, — | Kuman itu selalu kritis, antibiotik apa pun |
| —, `Meropenem`, `R` | Resisten Meropenem selalu kritis, kuman apa pun |
| `E. coli`, `Ceftriaxone`, `R` | Hanya kombinasi itu |

Baris yang **ketiganya kosong** ditolak (`VAL-106`): ia berarti seluruh hasil kritis, dan
**alarm yang berbunyi terus-menerus berhenti dibaca orang**.

### Sengaja NOL index unik atas kombinasinya

Berbeda dari breakpoint. Dua aturan yang tumpang tindih **bukan kesalahan**: *"MRSA selalu
kritis"* dan *"resisten Meropenem selalu kritis"* dapat berlaku bersamaan pada satu baris, dan
keduanya benar. Penilaian bersifat **cukup satu cocok**, sehingga tumpang tindih nol
menimbulkan keraguan — sedangkan dua breakpoint atas pasangan yang sama membuat hitungan
bergantung baris mana yang terbaca lebih dulu.

### `IsCritical` dihitung saat dibaca, `ComputedResult` disimpan — dan itu konsisten

Keduanya tampak bertentangan tetapi menjawab pertanyaan berbeda:

| Nilai | Disimpan? | Pertanyaannya |
|---|---|---|
| `IsCritical` | **Tidak** | *"Apakah ini berbahaya menurut aturan hari ini"* — masa kini |
| `ComputedResult` | **Ya** | *"Apa yang sistem katakan saat analis menimpanya"* — masa lalu |

---

## 2. Verifikasi — terhadap aplikasi yang berjalan

Satu baris kepekaan disiapkan: *Branhamella catarrhalis* × *Ampicillin*, zona `10` terhadap
rentang `13-17` → **`Resistant`**.

### `AC-166` — nol aturan: penanda mati, DAN layar dapat menyatakannya

```text
criticalRuleAvailable = false   result = Resistant   isCritical = false
```

✅ **Terbukti.** Hasil `Resistant` **tidak** menyalakan penanda, dan
`criticalRuleAvailable: false` memberi layar cara menyatakan *"aturannya belum disetel"* —
bukan diam.

> Inilah bagian yang paling mudah dilewatkan. Tanpa ruas kedua itu, layar bersih terbaca
> sebagai *"hasil aman"* padahal yang terjadi adalah `DR-LAB-002` belum mengisi satu pun
> baris aturan.

### `VAL-106` — aturan tanpa penilai ditolak

`POST` hanya berisi `ruleNote` → **`400`**
*"Aturan kritis harus menyebut sedikitnya organisme, antibiotik, atau hasil kepekaan."* ✅

### `AC-167` — aturan ADA tetapi tidak cocok

Ditambahkan: *"Kritis bila Branhamella catarrhalis diuji terhadap Ampicillin dengan
**Sensitive**."* Hasilnya `Resistant`, jadi tidak cocok.

```text
criticalRuleAvailable = true   result = Resistant   isCritical = false
```

✅ **Terbukti.** Keadaan ini **berbeda** dari `AC-166` meski penandanya sama-sama mati —
dan perbedaannya terbaca dari `criticalRuleAvailable`.

### Aturan yang cocok menyalakan penanda

Ditambahkan aturan **luas**: *"Kritis bila **kuman apa saja** diuji terhadap Ampicillin dengan
**Resistant**."*

```text
criticalRuleAvailable = true   result = Resistant   isCritical = true
```

✅ Ruas organisme yang dikosongkan terbukti berarti "apa saja".

### Daftar aturan

`totalData: 2`, dan `ruleSummary` menuliskan bunyinya dalam kalimat yang dapat dibaca orang —
termasuk *"kuman apa saja"* untuk ruas yang dikosongkan.

---

## 3. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| **Pengisian baris aturan** | `LAB-DEC-103` butir 4 menyerahkannya kepada **`DR-LAB-002`**. Dua baris uji di bawah **bukan** aturan klinis |
| Alur pelaporan nilai kritis | `S5`, tertahan `LAB-P0-004` dan `LAB-OPEN-014` |
| Pemilihan dokter konfirmator | `BE-LAB-59` |
| `git add`, `commit`, `push` | Nol diminta |

> **Dua baris uji tertinggal di dev**, dan keduanya **wajib dihapus** sebelum dipakai
> sungguhan: *"Branhamella catarrhalis × Ampicillin × Sensitive"* dan *"kuman apa saja ×
> Ampicillin × Resistant"*. Keduanya dibuat untuk membuktikan `AC-167`, dan yang kedua
> **terlalu luas** untuk dibiarkan hidup.

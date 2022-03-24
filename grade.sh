branch=$1

generateReportAndBadges() {
    rm -f *.svg
    printf "# Rapport de notation\n\n" > report.md

    qualityRate=$(computeQuality)
    releaseGrade=$(computeReleases)
    completeGrade=$(computeCompletion)
    computeTotal "$releaseGrade" "$completeGrade" "$qualityRate"
}

computeQuality() {
    if [ ! -f "quality.md" ]; then
        createDefaultQuality
    fi

    if [ -f "coverage/line_rate.txt" ]; then 
        coverageRate=$(cat "coverage/line_rate.txt")
    else
        coverageRate="0"
    fi
    coveragePercentage=$(makePercentage $coverageRate)
    makeBadge "couverture" "$coveragePercentage%25"

    echo $coverageRate | sed '1s/^/* Couverture de tests: /' >> quality.md

    printf "#" >> report.md
    cat quality.md >> report.md
    printf "\n" >> report.md
    
    qualityRate=$(cat quality.md | grep "^\\* " | awk -F": " '{ total += $2; count++ } END { print total/count }')
    qualityPercentage=$(makePercentage $qualityRate)

    printf "Coefficient de qualité = $qualityPercentage%%\n\n" >> report.md
    makeBadge "qualite" "$qualityPercentage%25"

    echo $qualityRate
}

createDefaultQuality() {
    cat <<-EOF > quality.md
# Qualité

* Nommage variables: 1
* Usage adéquat des fonctionnalités du langage: 1
* Simplicité des fonctions: 1
* Simplicité des classes: 1
* Pas de dupplication de code: 1
* Séparation de responsabilités: 1
* Encapsulation: 1
EOF
}

computeReleases() {
    versionCount=0
    rateSum=0
    printf "## Versions\n\n" >> report.md
    for version in ./tests/version*.txt; do
        [ -f "$version" ] || continue

        versionNumber=$(printf $version | sed -e 's/.\/tests\/version\([0-9]*\)\.txt/\1/')

        printf "### Version $versionNumber\n\n" >> report.md
        rate=$(processTestFile $version)
        rateSum=$(awk "BEGIN {print $rateSum + $rate; exit}")
        versionCount=$(($versionCount + 1))
    done

    printf "### Total rendus\n\n" >> report.md    
    if [ $versionCount -gt 0 ]; then
        averageRate=$(awk "BEGIN {print $rateSum/$versionCount; exit}")
    else
        printf "**Aucun test de version**\n\n" >> report.md  
        averageRate=0
    fi

    printf "Taux moyen = $averageRate\n\n" >> report.md    
    
    grade=$(processHalfGrade "$averageRate*10" "livraison" "yellow")
    echo $grade
}

computeCompletion() {
    printf "## Rendu complet\n\n" >> report.md

    if [ -f "./tests/full.txt" ]; then
        completeRate=$(processTestFile "./tests/full.txt")
    else
        printf "**Aucun test complet**\n\n" >> report.md    
        completeRate=0
    fi
    complete=$(processHalfGrade "$completeRate*10" "completude" "organge")

    completePercentage=$(makePercentage $completeRate)
    makeBadge "completude" "$completePercentage%25"

    echo $complete
}

processTestFile() {
    sed -e 's/^/* /' $1 >> report.md
    rate=$(cut -d":" -f2 $1 | cut -d" " -f2 | sed ':a;N;$!ba;s/\n/:/g' | awk -F: '{ if ($1 > 0) print $2/$1; else print 0; }')
    printf "* Taux: $rate\n\n" >> report.md

    echo $rate
}

processHalfGrade() {
    grade=$(makeGrade $1)
    printf "Note intermédiaire = $grade/10\n\n" >> report.md

    echo $grade
}

computeTotal() {
    subTotalWithoutQuality=$(awk "BEGIN {print $1+$2; exit}")
    qualityImpact="0.5 + $3 / 2.0"
    qualityPercentage=$(makePercentage "$qualityImpact")
    total=$(makeGrade "$subTotalWithoutQuality*($qualityImpact)")

    printf "## Total \n\n" >> report.md
    printf "Sous-total: $subTotalWithoutQuality/20 \n\n" >> report.md
    printf "Impact qualité: $qualityPercentage%%\n\n" >> report.md
    printf "Total: $total/20 \n\n" >> report.md

    makeBadge "note" "$total%2F20"
}

makeGrade() {
    echo $(awk "BEGIN {print int(100*($1) + 0.5)/100; exit}")
}

makePercentage() {
    echo $(awk "BEGIN {print int(10000*($1) + 0.5)/100; exit}")
}

makeBadge() {
    color=$(chooseBadgeColor "$branch" "$1")
    
    if [ ! "$color" = "" ]; then
        curl -sS "https://img.shields.io/badge/$1%20de%20$branch-$2-$color.svg" > $1.svg
    fi
}

chooseBadgeColor() {
    case "$1/$2" in
        "prod/note") echo "e05d44";; # bright red
        "master/note") echo "a3311b";; # dark red
        "prod/completude") echo "fe7d37";; # bright orange
        "master/completude") echo "ce4901";; # dark orange
        "prod/livraison") echo "dfb317";; # bright yellow
        "master/livraison") echo "83690d";; # dark yellow
        "prod/qualite") echo "007ec6";; # bright blue
        "master/qualite") echo "003d60";; # dark blue
        "prod/couverture") echo "97ca00";; # bright green
        "master/couverture") echo "4b6400";; # dark green
    esac
}

generateReportAndBadges